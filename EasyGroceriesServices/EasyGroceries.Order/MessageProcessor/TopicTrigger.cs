using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Order.Model.Context;
using EasyGroceries.Order.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Address = EasyGroceries.Order.Model.Entities.Address;

namespace EasyGroceries.Order.MessageProcessor;

public class TopicTrigger
{
    private readonly ILogger<TopicTrigger> _logger;
    private readonly OrderContext _context;

    public TopicTrigger(ILogger<TopicTrigger> logger, OrderContext context)
    {
        _logger = logger;
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
    
    private async Task ProcessUserCreatedEvent(UserCreatedEvent eventToProcess)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == eventToProcess.Id);
        if (user != null)
        {
            _logger.LogError("User already exists: {@ExistingUser} - {@Event}", user, eventToProcess);
            return;
        }

        var newUser = new User
        {
            Id = eventToProcess.Id,
            FirstName = eventToProcess.FirstName,
            LastName = eventToProcess.LastName,
            Email = eventToProcess.Email,
            PhoneNumber = eventToProcess.PhoneNumber,
            DefaultDeliveryAddress = new Address
            {
                Line1 = eventToProcess.DefaultDeliveryAddress!.Line1,
                Line2 = eventToProcess.DefaultDeliveryAddress!.Line2,
                Line3 = eventToProcess.DefaultDeliveryAddress!.Line3,
                City = eventToProcess.DefaultDeliveryAddress!.City,
                County = eventToProcess.DefaultDeliveryAddress!.County,
                Postcode = eventToProcess.DefaultDeliveryAddress!.Postcode,
                Country = eventToProcess.DefaultDeliveryAddress!.Country,
                CountryCode = eventToProcess.DefaultDeliveryAddress!.CountryCode
            },
            DefaultBillingAddress = new Address
            {
                Line1 = eventToProcess.DefaultBillingAddress!.Line1,
                Line2 = eventToProcess.DefaultBillingAddress!.Line2,
                Line3 = eventToProcess.DefaultBillingAddress!.Line3,
                City = eventToProcess.DefaultBillingAddress!.City,
                County = eventToProcess.DefaultBillingAddress!.County,
                Postcode = eventToProcess.DefaultBillingAddress!.Postcode,
                Country = eventToProcess.DefaultBillingAddress!.Country,
                CountryCode = eventToProcess.DefaultBillingAddress!.CountryCode
            }
        };

        _logger.LogInformation("Saving User: {@User}", user);

        _context.Users.Add(newUser);
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
        var order = await _context.Orders.Include(order => order.Items).FirstOrDefaultAsync(o =>
            o.BasketId == eventToProcess.BasketId && o.UserId == eventToProcess.UserId);
        
        var user = await _context.Users.Include(user => user.DefaultDeliveryAddress).FirstOrDefaultAsync(u => u.Id == eventToProcess.UserId);
        if (user == null)
        {
            _logger.LogError("User with Id: {UserId} not found", eventToProcess.UserId);
            throw new Exception($"User with id: {eventToProcess.UserId} not found");
        }

        if (order == null)
        {
            order = new Model.Entities.Order
            {
                BasketId = eventToProcess.BasketId,
                UserId = eventToProcess.UserId,
                Items = new List<OrderItem>(),
                DeliveryAddress = new Address
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

            order = (await _context.Orders.AddAsync(order)).Entity;
            await _context.SaveChangesAsync();
        }

        if (!order.Items.Exists(i => i.Name.ToLowerInvariant() == eventToProcess.Name.ToLowerInvariant()))
        {
            order.Items.Add(new OrderItem
            {
                Name = eventToProcess.Name,
                Description = eventToProcess.Description,
                IncludeInDelivery = eventToProcess.IncludeInDelivery,
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
            });

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}