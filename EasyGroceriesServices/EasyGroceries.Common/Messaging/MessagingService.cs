using Azure.Messaging.ServiceBus;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyGroceries.Common.Messaging;

public class MessagingService : IMessagingService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<MessagingService> _logger;

    public MessagingService(ServiceBusClient serviceBusClient, ILogger<MessagingService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }
    
    public async Task EmitEvent(IEvent baseEvent)
    {
        _logger.LogInformation("Emitting event: {@Message}", baseEvent);

        var topicName = baseEvent.GetTopicName();
        _logger.LogInformation("Identified Topic to emit on: {TopicName}", topicName);
        
        var sender = _serviceBusClient.CreateSender(topicName);
        var message = new ServiceBusMessage(JsonConvert.SerializeObject(baseEvent));
        message.ApplicationProperties.Add("EventType", baseEvent.Type.ToString());

        if (string.IsNullOrWhiteSpace(message.CorrelationId))
        {
            _logger.LogInformation("Set CorrelationId");
            message.CorrelationId = baseEvent.CorrelationId ?? Guid.NewGuid().ToString();
        }

        await sender.SendMessageAsync(message);
        _logger.LogInformation("Event emitted successfully");
    }
}