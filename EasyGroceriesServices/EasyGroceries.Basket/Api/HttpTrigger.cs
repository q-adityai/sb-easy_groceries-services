using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.Basket.Configuration;
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
using Microsoft.Extensions.Options;

namespace EasyGroceries.Basket.Api;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IMessagingService _messagingService;
    private readonly BasketContext _basketContext;
    private readonly BasketApiOptions _options;

    public HttpTrigger(ILogger<HttpTrigger> logger, IMessagingService messagingService, BasketContext basketContext, IOptions<BasketApiOptions> options)
    {
        _logger = logger;
        _messagingService = messagingService;
        _basketContext = basketContext;
        _options = options.Value;
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
        else if (product.Category == ProductCategory.PromotionCoupon && basketProductDto.Quantity > 1)
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
            basket = await _basketContext.Baskets.FirstOrDefaultAsync(b => b.Id == basketProductDto.BasketId);
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
                Id = $"{Constants.BasketPrefix}{Guid.NewGuid()}",
                UserId = basketProductDto.UserId,
                Status = BasketStatus.Empty,
                CreatedAt = DateTimeOffset.UtcNow
            };

            basket = (await _basketContext.AddAsync(basket)).Entity;
            await _basketContext.SaveChangesAsync();
        }
        
        //We have a basket with us, we can add products to it
        
        //If the product already exists, we only amend the quantity, else we add the product and set the quantity
        var existingBasketProduct =
            await _basketContext.BasketProducts.FirstOrDefaultAsync(bp =>
                bp.BasketId == basket.Id && bp.ProductId == product.Id);
        if (existingBasketProduct != null)
        {
            if (existingBasketProduct.Category == ProductCategory.PromotionCoupon)
            {
                //User has already applied a promotion, don't allow to add again
                var errorMessage = $"Cannot apply multiple promotions";
                _logger.LogError("{Message}", errorMessage);
                return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
            }
            
            existingBasketProduct.Quantity += basketProductDto.Quantity;
            _basketContext.Update(existingBasketProduct);
            await _basketContext.SaveChangesAsync();
        }
        else
        {
            var newBasketProduct = new BasketProduct
            {
                ProductId = product.Id,
                SkuCode = product.Sku.Code,
                Category = product.Category,
                IncludeInDelivery = product.Category == ProductCategory.PromotionCoupon,
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
                DiscountPercentInMinorUnits = product.AppliedDiscountPercentInMinorUnits,
                DiscountApplicable = product.DiscountApplicable,
                Quantity = basketProductDto.Quantity,
                BasketId = basket.Id
            };
            await _basketContext.AddAsync(newBasketProduct);
            await _basketContext.SaveChangesAsync();
        }
        
        //Now that we have perform the required operation, we can save the basket
        basket.Status = BasketStatus.Active;
        _basketContext.Baskets.Update(basket);
        await _basketContext.SaveChangesAsync();
        
        return new OkObjectResult(StandardResponse.Success(basket));
    }
    
    [FunctionName("RemoveProductFromBasketAsync")]
    public async Task<IActionResult> RemoveProductFromBasketAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Baskets/Product/Remove")] HttpRequest req)
    {
        var basketProductDto = await req.GetBody<BasketProductRequestDto>();
        _logger.LogInformation("Received request to process: {MethodName} with input: {@Input}", nameof(RemoveProductFromBasketAsync), basketProductDto);

        if (!DtoValidation.IsValid(basketProductDto, out var result)) return result;
        
        if (string.IsNullOrWhiteSpace(basketProductDto.BasketId))
        {
            var errorMessage = "The BasketId field is required";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

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

        var basket = await _basketContext.Baskets.FirstOrDefaultAsync(u => u.Id == basketProductDto.BasketId);
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
        var existingBasketProduct = await
            _basketContext.BasketProducts.FirstOrDefaultAsync(bp =>
                bp.BasketId == basket.Id && bp.ProductId == product.Id);
        if (existingBasketProduct == null)
        {
            var errorMessage = $"Intended product to remove not found";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        //If the supplied quantity to remove is higher than the present quantity, we cannot proceed
        if (basketProductDto.Quantity > existingBasketProduct.Quantity)
        {
            var errorMessage = $"Insufficient quantity to remove";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        //If the supplied quantity to remove matches the present quantity then we will remove the product line altogether else, just amend the quantity
        if (basketProductDto.Quantity == existingBasketProduct.Quantity)
        {
            _basketContext.Remove(existingBasketProduct);
            await _basketContext.SaveChangesAsync();
        }
        else
        {
            existingBasketProduct.Quantity -= basketProductDto.Quantity;
            _basketContext.Update(existingBasketProduct);
            await _basketContext.SaveChangesAsync();
        }

        //If
        basket.Status = (await _basketContext.BasketProducts.CountAsync(bp => bp.BasketId == basket.Id)) == 0 ? BasketStatus.Empty : BasketStatus.Active;

        //Now that we have perform the required operation, we can save the basket
        _basketContext.Update(basket);
        await _basketContext.SaveChangesAsync();
        
        return new OkObjectResult(StandardResponse.Success(basket));
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

        var basket = await _basketContext.Baskets.FirstOrDefaultAsync(u => u.Id == basketId);
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

        if ((await _basketContext.BasketProducts.CountAsync(bp => bp.BasketId == basket.Id && bp.Category == ProductCategory.PromotionCoupon)) > 0)
        {
            //There is a promotion present, calculate the discounts
            var discountPercentInMinorUnits = _options.DefaultDiscountPercentInMinorUnits;
            foreach (var product in _basketContext.BasketProducts.Where(bp => bp.BasketId == basket.Id && bp.Category != ProductCategory.PromotionCoupon && bp.DiscountApplicable))
            {
                product.DiscountPercentInMinorUnits = discountPercentInMinorUnits;
                product.DiscountedPrice.AmountInMinorUnits = (long)(product.Price.AmountInMinorUnits * ((100 - (discountPercentInMinorUnits / 100.00))/100));
                _basketContext.Update(product);
                await _basketContext.SaveChangesAsync();
            }
        }
        basket.Status = BasketStatus.CheckedOut;
        _basketContext.Baskets.Update(basket);
        await _basketContext.SaveChangesAsync();

        var productCheckedOutEvents = new List<ProductCheckedOutEvent>();
        foreach (var basketProduct in await _basketContext.BasketProducts.Where(bp => bp.BasketId == basket.Id).ToListAsync())
        {
            productCheckedOutEvents.Add(new ProductCheckedOutEvent
            {
                BasketId = basket.Id,
                UserId = basket.UserId,
                ProductId = basketProduct.ProductId,
                Name = basketProduct.Name,
                Description = basketProduct.Description,
                Quantity = basketProduct.Quantity,
                Price = basketProduct.Price,
                DiscountedPrice = basketProduct.DiscountedPrice,
                DiscountPercentInMinorUnits = basketProduct.DiscountPercentInMinorUnits,
                IncludeInDelivery = basketProduct.IncludeInDelivery
            });
        }

        await _messagingService.EmitEventsAsync(productCheckedOutEvents);

        return new OkObjectResult(StandardResponse.Success(basket));
    }
    
    
    [FunctionName("BasketPreviewAsync")]
    public async Task<IActionResult> BasketPreviewAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Baskets/{basketId}/preview")] HttpRequest req, string basketId)
    {
        _logger.LogInformation("Received request to process: {MethodName} with input: {@Input}", nameof(BasketPreviewAsync), basketId);

        if (string.IsNullOrWhiteSpace(basketId))
        {
            var errorMessage = $"BasketId cannot be null or empty";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var basket = await _basketContext.Baskets.FirstOrDefaultAsync(b => b.Id == basketId);
        if (basket == null)
        {
            //We dont have a basket with the supplied Id, no point processing further
            var errorMessage = $"Basket with basketId: {basketId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var user = await _basketContext.Users.Include(user => user.DefaultDeliveryAddress).FirstOrDefaultAsync(u => u.Id == basket.UserId);
        if (user == null)
        {
            var errorMessage = $"User with userId: {basket.UserId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }

        var basketProducts = await _basketContext.BasketProducts.Where(bp => bp.BasketId == basket.Id).ToListAsync();

        var basketPreviewDto = new BasketPreviewDto
        {
            BasketId = basket.Id,
            UserId = user.Id,
            BasketValue = new Money
            {
                Currency = Currency.Gbp,
                AmountInMinorUnits = basketProducts.Sum(p => p.DiscountedPrice.AmountInMinorUnits * p.Quantity)
            },
            Products = basketProducts.Select(bp => new BasketProductPreviewDto
            {
                Name = bp.Name,
                Description = bp.Description,
                IncludeInDelivery = bp.IncludeInDelivery,
                Price = new Money
                {
                    Currency = bp.Price.Currency,
                    AmountInMinorUnits = bp.Price.AmountInMinorUnits
                },
                DiscountedPrice = new Money
                {
                    Currency = bp.DiscountedPrice.Currency,
                    AmountInMinorUnits = bp.DiscountedPrice.AmountInMinorUnits
                },
                DiscountPercentInMinorUnits = bp.DiscountPercentInMinorUnits,
                Quantity = bp.Quantity
            }).ToList()
        };
        
        return new OkObjectResult(StandardResponse.Success(basketPreviewDto));
    }
}