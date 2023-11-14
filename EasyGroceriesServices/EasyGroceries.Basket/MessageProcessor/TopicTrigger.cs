using System.Threading.Tasks;
using AutoMapper;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyGroceries.Basket.MessageProcessor;

public class TopicTrigger
{
    private readonly ILogger<TopicTrigger> _logger;
    private readonly IMapper _mapper;
    private readonly BasketContext _context;

    public TopicTrigger(ILogger<TopicTrigger> logger, IMapper mapper, BasketContext context)
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
    }

    [FunctionName("UserSubscriber")]
    public async Task UserSubscriberAsync(
        [ServiceBusTrigger("%UserTopicName%", "%UserTopicSubscriptionName%", Connection = "MessagingConnection")]
        string mySbMsg)
    {
        _logger.LogInformation("Received message: {Message}", mySbMsg);

        var baseEvent = JsonConvert.DeserializeObject<BaseEvent>(mySbMsg);
        switch (baseEvent!.Type)
        {
            case EventType.UserCreated:
            {
                await ProcessUserCreatedEvent(JsonConvert.DeserializeObject<UserCreatedEvent>(mySbMsg)!);
                break;
            }
            default:
            {
                _logger.LogError("Unknown event received");
                return;
            }
        }
    }

    [FunctionName("InventorySubscriber")]
    public async Task InventorySubscriberAsync(
        [ServiceBusTrigger("%InventoryTopicName%", "%InventoryTopicSubscriptionName%",
            Connection = "MessagingConnection")]
        string mySbMsg)
    {
        _logger.LogInformation("Received message: {Message}", mySbMsg);

        var baseEvent = JsonConvert.DeserializeObject<BaseEvent>(mySbMsg);
        switch (baseEvent!.Type)
        {
            case EventType.ProductCreated:
            {
                await ProcessProductCreatedEvent(JsonConvert.DeserializeObject<ProductCreatedEvent>(mySbMsg)!);
                break;
            }
            default:
            {
                _logger.LogError("Unknown event received");
                return;
            }
        }
    }


    private async Task ProcessUserCreatedEvent(UserCreatedEvent eventToProcess)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == eventToProcess.Id);
        if (existingUser != null)
        {
            _logger.LogError("User already exists: {@ExistingUser} - {@Event}", existingUser, eventToProcess);
            return;
        }
        
        var user = _mapper.Map<User>(eventToProcess);

        _logger.LogInformation("Saving User: {@User}", user);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    private async Task ProcessProductCreatedEvent(ProductCreatedEvent eventToProcess)
    {
        var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == eventToProcess.Id);
        if (existingProduct != null)
        {
            _logger.LogError("Product already exists: {@ExistingProduct} - {@Event}", existingProduct, eventToProcess);
            return;
        }
        
        var product = _mapper.Map<Product>(eventToProcess);

        _logger.LogInformation("Saving Product: {Product}", JsonConvert.SerializeObject(product));
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }
}