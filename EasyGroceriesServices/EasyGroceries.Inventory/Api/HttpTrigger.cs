using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Common.Utils;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Model.Entities;
using EasyGroceries.Inventory.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace EasyGroceries.Inventory.Api;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IMapper _mapper;
    private readonly IMessagingService _messagingService;
    private readonly IProductRepository _productRepository;

    public HttpTrigger(IMapper mapper, ILogger<HttpTrigger> logger, IProductRepository productRepository,
        IMessagingService messagingService)
    {
        _mapper = mapper;
        _logger = logger;
        _productRepository = productRepository;
        _messagingService = messagingService;
    }

    [FunctionName("GetApplicableProductsAsync")]
    public async Task<IActionResult> GetApplicableProductsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Products/Applicable")]
        HttpRequest req)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName}", nameof(GetApplicableProductsAsync));

        return new OkObjectResult(
            StandardResponse.Success(
                _mapper.Map<List<ProductDto>>(await _productRepository.GetAllApplicableProductsAsync())));
    }

    [FunctionName("GetProductsAsync")]
    public async Task<IActionResult> GetProductsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Products")]
        HttpRequest req)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName}", nameof(GetProductsAsync));

        return new OkObjectResult(
            StandardResponse.Success(_mapper.Map<List<ProductDto>>(await _productRepository.GetProductsAsync())));
    }

    [FunctionName("CreateProductAsync")]
    public async Task<IActionResult> CreateProductAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Products")]
        HttpRequest req)
    {
        var product = await req.GetBody<ProductDto>();

        _logger.LogInformation("Processing request for MethodName: {MethodName} with input: {@Input}",
            nameof(CreateProductAsync), product);

        if (!DtoValidation.IsValid(product, out var result)) return result;

        var existingProduct = await _productRepository.GetProductByNameAsync(product.Name);
        if (existingProduct != null)
        {
            var errorMessage = "Product with same name already exists. Use a different name.";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        var dbResponse = await _productRepository.CreateProductAsync(_mapper.Map<Product>(product));

        await _messagingService.EmitEvent(_mapper.Map<ProductCreatedEvent>(dbResponse));

        return new CreatedResult(new Uri(req.Path.ToUriComponent()),
            StandardResponse.Success(_mapper.Map<ProductDto>(dbResponse)));
    }
}