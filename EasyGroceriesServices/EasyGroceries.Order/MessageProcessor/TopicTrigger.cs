using System;
using System.Threading.Tasks;
using AutoMapper;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Order.Model.Context;
using EasyGroceries.Order.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyGroceries.Order.MessageProcessor;

public class TopicTrigger
{
    private readonly ILogger<TopicTrigger> _logger;
    private readonly IMapper _mapper;
    private readonly OrderContext _context;

    public TopicTrigger(ILogger<TopicTrigger> logger, IMapper mapper, OrderContext context)
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
            case EventType.UserUpdated:
            {
                await ProcessUserUpdatedEvent(JsonConvert.DeserializeObject<UserUpdatedEvent>(mySbMsg)!);
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
    
    private async Task ProcessUserUpdatedEvent(UserUpdatedEvent eventToProcess)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == eventToProcess.Id);
        if (existingUser == null)
        {
            var newUser = _mapper.Map<User>(eventToProcess);
            await _context.AddAsync(newUser);
        }
        else
        {
            existingUser.FirstName = eventToProcess.FirstName;
            existingUser.LastName = eventToProcess.LastName;
            existingUser.Email = eventToProcess.Email;
            existingUser.PhoneNumber = eventToProcess.PhoneNumber;
            existingUser.DefaultBillingAddress = eventToProcess.DefaultBillingAddress.Clone();
            existingUser.DefaultDeliveryAddress = eventToProcess.DefaultDeliveryAddress.Clone();
            _context.Update(existingUser);
        }

        await _context.SaveChangesAsync();
    }
    
    [FunctionName("BasketSubscriber")]
    public async Task BasketSubscriberAsync([ServiceBusTrigger("%BasketTopicName%", "%BasketTopicSubscriptionName%", Connection = "MessagingConnection")] string mySbMsg)
    {
        _logger.LogInformation("Received message: {Message}", mySbMsg);

        var baseEvent = JsonConvert.DeserializeObject<BaseEvent>(mySbMsg);
        switch (baseEvent!.Type)
        {
            case EventType.ProductCheckedOut:
            {
                await ProcessProductCheckedOutEvent(JsonConvert.DeserializeObject<ProductCheckedOutEvent>(mySbMsg)!);
                break;
            }
            default:
            {
                _logger.LogError("Unknown event received");
                return;
            }
        }
    }

    private async Task ProcessProductCheckedOutEvent(ProductCheckedOutEvent eventToProcess)
    {
        var existingOrder = await _context.Orders.FirstOrDefaultAsync(o =>
            o.BasketId == eventToProcess.BasketId && o.UserId == eventToProcess.UserId);
        
        var user = await _context.Users.Include(user => user.DefaultDeliveryAddress).FirstOrDefaultAsync(u => u.Id == eventToProcess.UserId);
        if (user == null)
        {
            _logger.LogError("User with Id: {UserId} not found", eventToProcess.UserId);
            throw new Exception($"User with id: {eventToProcess.UserId} not found");
        }

        if (existingOrder == null)
        {
            var newOrder = new Model.Entities.Order
            {
                BasketId = eventToProcess.BasketId,
                UserId = eventToProcess.UserId,
                DeliveryAddress = new DefaultAddress()
                {
                    Line1 = user.DefaultDeliveryAddress!.Line1,
                    Line2 = user.DefaultDeliveryAddress!.Line2,
                    Line3 = user.DefaultDeliveryAddress!.Line3,
                    City = user.DefaultDeliveryAddress!.City,
                    County = user.DefaultDeliveryAddress!.County,
                    Postcode = user.DefaultDeliveryAddress!.Postcode,
                    Country = user.DefaultDeliveryAddress!.Country,
                    CountryCode = user.DefaultDeliveryAddress!.CountryCode
                }
            };

            existingOrder = (await _context.Orders.AddAsync(newOrder)).Entity;
            await _context.SaveChangesAsync();
        }

        var existingOrderItem = await _context.OrderItems.FirstOrDefaultAsync(oi =>
            oi.OrderId == existingOrder.Id && oi.ProductId == eventToProcess.ProductId);
        if (existingOrderItem == null)
        {
            var newOrderItem = new OrderItem
            {
                Id = $"{Constants.OrderPrefix}{Constants.ProductPrefix}{Guid.NewGuid()}",
                Name = eventToProcess.Name,
                Description = eventToProcess.Description,
                IncludeInDelivery = eventToProcess.IncludeInDelivery,
                ProductId = eventToProcess.ProductId,
                OrderId = existingOrder.Id,
                Price = new Money
                {
                    Currency = eventToProcess.Price.Currency,
                    AmountInMinorUnits = eventToProcess.Price.AmountInMinorUnits
                },
                DiscountedPrice = new Money
                {
                    Currency = eventToProcess.DiscountedPrice.Currency,
                    AmountInMinorUnits = eventToProcess.DiscountedPrice.AmountInMinorUnits
                },
                DiscountPercentInMinorUnits = eventToProcess.DiscountPercentInMinorUnits,
                Quantity = eventToProcess.Quantity
            };
            
            await _context.AddAsync(newOrderItem);
            await _context.SaveChangesAsync();
        }
    }
}