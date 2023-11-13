using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyGroceries.Basket.Dto;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Basket.Repositories.Interfaces;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EasyGroceries.Basket.Api;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IBasketRepository _basketRepository;

    public HttpTrigger(ILogger<HttpTrigger> logger, IUserRepository userRepository, IProductRepository productRepository, IBasketRepository basketRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _basketRepository = basketRepository;
    }
    
    [FunctionName("AddProductToBasketAsync")]
    public async Task<IActionResult> AddProductToBasketAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Baskets/Product/Add")] HttpRequest req)
    {
        var basketProductDto = await req.GetBody<BasketProductRequestDto>();
        _logger.LogInformation("Received request to process: {MethodName} with input: {@Input}", nameof(AddProductToBasketAsync), basketProductDto);

        if (!DtoValidation.IsValid(basketProductDto, out var result)) return result;

        var user = await _userRepository.GetUserById(basketProductDto.UserId);
        if (user == null)
        {
            var errorMessage = $"User with userId: {basketProductDto.UserId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var product = await _productRepository.GetProductById(basketProductDto.ProductId);
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
            basket = await _basketRepository.GetBasketAsync(basketProductDto.BasketId);
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

            basket = await _basketRepository.CreateBasketAsync(basket);
        }
        
        //We have a basket with us, we can add products to it
        
        //If the product already exists, we only amend the quantity, else we add the product and set the quantity
        if (basket.Products.Exists(p => p.Product.Id == product.Id))
        {
            var index = basket.Products.FindIndex(p => p.Product.Id == product.Id);
            
            var entry = basket.Products[index];
            if (entry.Product.CategoryName == ProductCategory.PromotionCoupon.ToString())
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
                Product = product,
                Quantity = basketProductDto.Quantity
            });
        }
        
        //Now that we have perform the required operation, we can save the basket
        basket.Status = BasketStatus.Active;

        await _basketRepository.SaveBasketAsync(basket);
        
        return new OkObjectResult(StandardResponse.Success(basket.Id));
    }
    
    [FunctionName("RemoveProductFromBasketAsync")]
    public async Task<IActionResult> RemoveProductFromBasketAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Baskets/Product/Remove")] HttpRequest req)
    {
        var basketProductDto = await req.GetBody<BasketProductRequestDto>();
        _logger.LogInformation("Received request to process: {MethodName} with input: {@Input}", nameof(RemoveProductFromBasketAsync), basketProductDto);

        if (!DtoValidation.IsValid(basketProductDto, out var result)) return result;

        var user = await _userRepository.GetUserById(basketProductDto.UserId);
        if (user == null)
        {
            var errorMessage = $"User with userId: {basketProductDto.UserId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var product = await _productRepository.GetProductById(basketProductDto.ProductId);
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

        var basket = await _basketRepository.GetBasketAsync(basketProductDto.BasketId);
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
        if (!basket.Products.Exists(p => p.Product.Id == product.Id))
        {
            var errorMessage = $"Intended product to remove not found";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }
        
        var index = basket.Products.FindIndex(p => p.Product.Id == product.Id);
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
        await _basketRepository.SaveBasketAsync(basket);
        
        return new OkObjectResult(StandardResponse.Success(basket.Id));
    }
}