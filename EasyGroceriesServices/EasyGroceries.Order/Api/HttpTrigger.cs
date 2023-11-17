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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.BasketId == basketId);
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

        var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
        var orderDto = new OrderDto()
        {
            OrderId = order.Id,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BasketValue = new Money
            {
                Currency = Currency.Gbp,
                AmountInMinorUnits = orderItems.Sum(p => p.DiscountedPrice.AmountInMinorUnits * p.Quantity)
            },
            Products = orderItems.Where(ot => ot.IncludeInDelivery).Select(ot => new OrderItemDto()
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
            DeliveryAddress = new DefaultAddress
            {
                Line1 = order.DeliveryAddress.Line1,
                Line2 = order.DeliveryAddress.Line2,
                Line3 = order.DeliveryAddress.Line3,
                City = order.DeliveryAddress.City,
                County = order.DeliveryAddress.County,
                Postcode = order.DeliveryAddress.Postcode,
                Country = order.DeliveryAddress.Country,
                CountryCode = order.DeliveryAddress.CountryCode
            }
        };
        
        return new OkObjectResult(StandardResponse.Success(orderDto));
    }
}