using System.Threading.Tasks;
using AutoMapper;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Basket.Repositories.Interfaces;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyGroceries.Basket.MessageProcessor;

public class TopicTrigger
{
    private readonly ILogger<TopicTrigger> _logger;
    private readonly IMapper _mapper;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public TopicTrigger(ILogger<TopicTrigger> logger, IMapper mapper, IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _userRepository = userRepository;
        _productRepository = productRepository;
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
        var existingUser = await _userRepository.GetUserById(eventToProcess.Id);
        if (existingUser != null)
        {
            _logger.LogError("User already exists: {@ExistingUser} - {@Event}", existingUser, eventToProcess);
            return;
        }
        
        var user = _mapper.Map<User>(eventToProcess);

        _logger.LogInformation("Saving User: {@User}", user);
        await _userRepository.CreateUserAsync(user);
    }

    private async Task ProcessProductCreatedEvent(ProductCreatedEvent eventToProcess)
    {
        var existingProduct = await _productRepository.GetProductById(eventToProcess.Id);
        if (existingProduct != null)
        {
            _logger.LogError("Product already exists: {@ExistingProduct} - {@Event}", existingProduct, eventToProcess);
            return;
        }
        
        var product = _mapper.Map<Product>(eventToProcess);

        _logger.LogInformation("Saving Product: {Product}", JsonConvert.SerializeObject(product));
        await _productRepository.AddProductAsync(product);
    }
}