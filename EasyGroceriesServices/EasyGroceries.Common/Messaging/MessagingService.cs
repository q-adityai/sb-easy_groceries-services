using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyGroceries.Common.Messaging;

public class MessagingService : IMessagingService
{
    private readonly ILogger<MessagingService> _logger;
    private readonly ServiceBusClient _serviceBusClient;

    public MessagingService(ServiceBusClient serviceBusClient, ILogger<MessagingService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task EmitEventAsync(IEvent baseEvent)
    {
        _logger.LogInformation("Emitting event: {Event}", JsonConvert.SerializeObject(baseEvent));

        var topicName = baseEvent.GetTopicName();
        _logger.LogInformation("Identified Topic to emit on: {TopicName}", topicName);

        var sender = _serviceBusClient.CreateSender(topicName);
        var message = new ServiceBusMessage(JsonConvert.SerializeObject(baseEvent));
        message.ApplicationProperties.Add("EventType", baseEvent.Type.ToString());

        if (string.IsNullOrWhiteSpace(message.CorrelationId))
        {
            var correlationId = baseEvent.CorrelationId ?? Guid.NewGuid().ToString();
            _logger.LogInformation("Set CorrelationId: {CorrelationId}", correlationId);
            message.CorrelationId = correlationId;
        }

        await sender.SendMessageAsync(message);
        _logger.LogInformation("Event emitted successfully");
    }
    
    /// <summary>
    /// Will emit multiple events of the same time. Determines the topic to send based on the first event
    /// </summary>
    /// <param name="baseEvents"></param>
    public async Task EmitEventsAsync<T>(List<T> baseEvents) where T: IEvent
    {
        _logger.LogInformation("Emitting {EventCount} events: {Event}", baseEvents.Count, JsonConvert.SerializeObject(baseEvents));
        
        if (baseEvents.Count == 0) return;
        
        var topicName = baseEvents[0].GetTopicName();
        _logger.LogInformation("Identified Topic to emit on: {TopicName}", topicName);

        var sender = _serviceBusClient.CreateSender(topicName);

        var serviceBusMessages = new List<ServiceBusMessage>();
        foreach (var baseEvent in baseEvents)
        {
            var message = new ServiceBusMessage(JsonConvert.SerializeObject(baseEvent));
            message.ApplicationProperties.Add("EventType", baseEvent.Type.ToString());

            if (string.IsNullOrWhiteSpace(message.CorrelationId))
            {
                var correlationId = baseEvent.CorrelationId ?? Guid.NewGuid().ToString();
                _logger.LogInformation("Set CorrelationId: {CorrelationId}", correlationId);
                message.CorrelationId = correlationId;
            }
            serviceBusMessages.Add(message);
        }

        await sender.SendMessagesAsync(serviceBusMessages);
        _logger.LogInformation("{EventCount} Events emitted successfully", baseEvents.Count);
    }
}