using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.Basket.Dto;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyGroceries.Basket.Api;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IMessagingService _messagingService;
    private readonly BasketContext _basketContext;

    public HttpTrigger(ILogger<HttpTrigger> logger, IMessagingService messagingService, BasketContext basketContext)
    {
        _logger = logger;
        _messagingService = messagingService;
        _basketContext = basketContext;
    }
    
    [FunctionName("AddProductToBasketAsync")]
    public async Task<IActionResult> AddProductToBasketAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Baskets/Product/Add")] HttpRequest req)
    {
        var basketProductDto = await req.GetBody<BasketProductRequestDto>();
        _logger.LogInformation("Received request to process: {MethodName} with input: {@Input}", nameof(AddProductToBasketAsync), basketProductDto);

        if (!DtoValidation.IsValid(basketProductDto, out var result)) return result;

        var user = await _basketContext.Users.FirstOrDefaultAsync(u => u.Id == basketProductDto.UserId);
        if (user == null)
        {
            var errorMessage = $"User with userId: {basketProductDto.UserId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var product = await _basketContext.Products.Include(product => product.Price)
            .Include(product => product.DiscountedPrice).FirstOrDefaultAsync(p => p.Id == basketProductDto.ProductId);
        if (product == null)
        {
            var errorMessage = $"Product with productId: {basketProductDto.ProductId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        else if (product.CategoryName == ProductCategory.PromotionCoupon.ToString() && basketProductDto.Quantity > 1)
        {
            //User is trying to apply multiple promotions, do not allow
            var errorMessage = $"Cannot apply multiple promotions";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }
        

        Model.Entities.Basket? basket;
        //We have a basket Id, we must validate if it belongs to the same user
        if (!string.IsNullOrWhiteSpace(basketProductDto.BasketId))
        {
            basket = await _basketContext.Baskets.Include(b => b.Products).FirstOrDefaultAsync(b => b.Id == basketProductDto.BasketId);
            if (basket == null)
            {
                //We dont have a basket with the supplied Id, no point processing further
                var errorMessage = $"Basket with basketId: {basketProductDto.BasketId} not found";
                _logger.LogError("{Message}", errorMessage);
                return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
            }

            if (basketProductDto.UserId != basket.UserId)
            {
                //Supplied userId is not the owner of the basket
                var errorMessage = $"Intended basket not found";
                _logger.LogError("{Message}", errorMessage);
                return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
            }
        }
        else
        {
            basket = new Model.Entities.Basket
            {
                UserId = basketProductDto.UserId,
                Products = new List<BasketProduct>(),
                Status = BasketStatus.Empty,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _basketContext.Baskets.Add(basket);
            await _basketContext.SaveChangesAsync();
        }
        
        //We have a basket with us, we can add products to it
        
        //If the product already exists, we only amend the quantity, else we add the product and set the quantity
        if (basket.Products.Exists(p => p.ProductId == product.Id))
        {
            var index = basket.Products.ToList().FindIndex(p => p.ProductId == product.Id);
            
            var entry = basket.Products[index];
            if (entry.CategoryName == ProductCategory.PromotionCoupon.ToString())
            {
                //User has already applied a promotion, don't allow to add again
                var errorMessage = $"Cannot apply multiple promotions";
                _logger.LogError("{Message}", errorMessage);
                return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
            }
            
            entry.Quantity += basketProductDto.Quantity;

            basket.Products[index] = entry;
        }
        else
        {
            basket.Products.Add(new BasketProduct
            {
                ProductId = product.Id,
                Sku = product.Sku,
                CategoryName = product.CategoryName,
                IncludeInDelivery = product.IncludeInDelivery,
                Name = product.Name,
                Description = product.Description,
                Price = new Money
                {
                    Currency = product.Price.Currency,
                    AmountInMinorUnits = product.Price.AmountInMinorUnits
                },
                DiscountedPrice = new Money
                {
                    Currency = product.DiscountedPrice.Currency,
                    AmountInMinorUnits = product.DiscountedPrice.AmountInMinorUnits
                },
                DiscountPercentInMinorUnits = product.DiscountPercentInMinorUnits,
                DiscountApplicable = product.DiscountApplicable,
                Quantity = basketProductDto.Quantity,

            });
        }
        
        //Now that we have perform the required operation, we can save the basket
        basket.Status = BasketStatus.Active;

        _basketContext.Baskets.Update(basket);
        await _basketContext.SaveChangesAsync();
        
        return new OkObjectResult(StandardResponse.Success(basket.Id));
    }
    
    [FunctionName("RemoveProductFromBasketAsync")]
    public async Task<IActionResult> RemoveProductFromBasketAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Baskets/Product/Remove")] HttpRequest req)
    {
        var basketProductDto = await req.GetBody<BasketProductRequestDto>();
        _logger.LogInformation("Received request to process: {MethodName} with input: {@Input}", nameof(RemoveProductFromBasketAsync), basketProductDto);

        if (!DtoValidation.IsValid(basketProductDto, out var result)) return result;

        var user = await _basketContext.Users.FirstOrDefaultAsync(u => u.Id == basketProductDto.UserId);
        if (user == null)
        {
            var errorMessage = $"User with userId: {basketProductDto.UserId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var product = await _basketContext.Products.FirstOrDefaultAsync(u => u.Id == basketProductDto.ProductId);
        if (product == null)
        {
            var errorMessage = $"Product with productId: {basketProductDto.ProductId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }

        if (string.IsNullOrWhiteSpace(basketProductDto.BasketId))
        {
            var errorMessage = "The BasketId field is required";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        var basket = await _basketContext.Baskets.Include(basket => basket.Products).FirstOrDefaultAsync(u => u.Id == basketProductDto.BasketId);
        if (basket == null)
        {
            //We dont have a basket with the supplied Id, no point processing further
            var errorMessage = $"Basket with basketId: {basketProductDto.BasketId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        if (basketProductDto.UserId != basket.UserId)
        {
            //Supplied userId is not the owner of the basket
            var errorMessage = $"Intended basket not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        //If the intended product does not exist in the basket then we need to return
        if (!basket.Products.Exists(p => p.ProductId == product.Id))
        {
            var errorMessage = $"Intended product to remove not found";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var index = basket.Products.FindIndex(p => p.ProductId == product.Id);
        var entry = basket.Products[index];

        //If the supplied quantity to remove is higher than the present quantity, we cannot proceed
        if (basketProductDto.Quantity > entry.Quantity)
        {
            var errorMessage = $"Insufficient quantity to remove";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        //If the supplied quantity to remove matches the present quantity then we will remove the product line altogether else, just amend the quantity
        if (basketProductDto.Quantity == entry.Quantity)
        {
            basket.Products.RemoveAt(index);
        }
        else
        {
            entry.Quantity -= basketProductDto.Quantity;
            basket.Products[index] = entry;
        }

        //If
        basket.Status = basket.Products.Count == 0 ? BasketStatus.Empty : BasketStatus.Active;

        //Now that we have perform the required operation, we can save the basket
        _basketContext.Baskets.Update(basket);
        await _basketContext.SaveChangesAsync();
        
        return new OkObjectResult(StandardResponse.Success(basket.Id));
    }

    [FunctionName("CheckoutBasketAsync")]
    public async Task<IActionResult> CheckoutBasketAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Baskets/Checkout/{basketId}")] HttpRequest req,
        string basketId)
    {
        _logger.LogInformation("Received request to process: {MethodName} with {Input}", nameof(CheckoutBasketAsync), basketId);

        if (string.IsNullOrWhiteSpace(basketId))
        {
            var errorMessage = $"basketId cannot be null";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        var basket = await _basketContext.Baskets.Include(basket => basket.Products)
            .ThenInclude(basketProduct => basketProduct.Price).Include(basket => basket.Products)
            .ThenInclude(basketProduct => basketProduct.DiscountedPrice).FirstOrDefaultAsync(u => u.Id == basketId);
        if (basket == null)
        {
            //We dont have a basket with the supplied Id, no point processing further
            var errorMessage = $"Basket with basketId: {basketId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }

        if (basket.Status != BasketStatus.Active)
        {
            //Basket is not in a state to be checked out
            var errorMessage = $"Cannot checkout basket as it is in the state: {basket.Status.ToString()}";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        if (basket.Products.Exists(p => p.CategoryName == ProductCategory.PromotionCoupon.ToString()))
        {
            //There is a promotion present, calculate the discounts
            var discountPercentInMinorUnits = 20;
            foreach (var product in basket.Products.Where(p => p.CategoryName != ProductCategory.PromotionCoupon.ToString() && p.DiscountApplicable))
            {
                product.DiscountPercentInMinorUnits = discountPercentInMinorUnits;
                product.DiscountedPrice.AmountInMinorUnits = product.Price.AmountInMinorUnits *
                                                                     ((100 - discountPercentInMinorUnits) / 100);
            }
        }

        basket.Status = BasketStatus.CheckedOut;

        _basketContext.Baskets.Update(basket);
        await _basketContext.SaveChangesAsync();

        var events = basket.Products.Select(basketProduct => new ProductCheckedOutEvent
        {
            BasketId = basket.Id,
            UserId = basket.UserId,
            ProductId = basketProduct.ProductId,
            Name = basketProduct.Name,
            Quantity = basketProduct.Quantity,
            Price = basketProduct.Price,
            DiscountedPrice = basketProduct.DiscountedPrice,
            DiscountPercentInMinorUnits = basketProduct.DiscountPercentInMinorUnits,
            IncludeInDelivery = basketProduct.IncludeInDelivery
        }).ToList();

        await _messagingService.EmitEventsAsync(events);

        return new OkObjectResult(StandardResponse.Success(basket.Id));
    }
}