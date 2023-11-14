using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Order.Dto;
using EasyGroceries.Order.Model.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyGroceries.Order.Api;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly OrderContext _context;

    public HttpTrigger(ILogger<HttpTrigger> logger, OrderContext context)
    {
        _logger = logger;
        _context = context;
    }
    [FunctionName("SubmitOrderAsync")]
    public async Task<IActionResult> SubmitOrderAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Orders/Submit/{basketId}")] HttpRequest req, string basketId)
    {
        _logger.LogInformation("Received request to process: {MethodName} with input: {@Input}", nameof(SubmitOrderAsync), basketId);

        if (string.IsNullOrWhiteSpace(basketId))
        {
            var errorMessage = $"BasketId cannot be null or empty";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var order = await _context.Orders.Include(order => order.Items).ThenInclude(orderItem => orderItem.Price)
            .Include(order => order.Items).ThenInclude(orderItem => orderItem.DiscountedPrice).FirstOrDefaultAsync(o => o.BasketId == basketId);
        if (order == null)
        {
            //We dont have a basket with the supplied Id, no point processing further
            var errorMessage = $"Order with basketId: {basketId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var user = await _context.Users.Include(user => user.DefaultDeliveryAddress).FirstOrDefaultAsync(u => u.Id == order.UserId);
        if (user == null)
        {
            var errorMessage = $"User with userId: {order.UserId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }

        var orderDto = new OrderDto()
        {
            OrderId = order.Id,
            UserId = user.Id,
            BasketValue = new Money
            {
                Currency = Currency.Gbp,
                AmountInMinorUnits = order.Items.Sum(p => p.DiscountedPrice.AmountInMinorUnits * p.Quantity)
            },
            Products = order.Items.Where(ot => ot.IncludeInDelivery).Select(ot => new OrderItemDto()
            {
                Name = ot.Name,
                Description = ot.Description,
                Price = new Money
                {
                    Currency = ot.Price.Currency,
                    AmountInMinorUnits = ot.Price.AmountInMinorUnits
                },
                DiscountedPrice = new Money
                {
                    Currency = ot.DiscountedPrice.Currency,
                    AmountInMinorUnits = ot.DiscountedPrice.AmountInMinorUnits
                },
                DiscountPercentInMinorUnits = ot.DiscountPercentInMinorUnits,
                Quantity = ot.Quantity
            }).ToList(),
            DeliveryAddress = new AddressDto
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
        
        return new OkObjectResult(StandardResponse.Success(orderDto));
    }
}