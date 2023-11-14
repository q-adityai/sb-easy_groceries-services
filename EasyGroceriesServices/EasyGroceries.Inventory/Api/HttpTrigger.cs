using System;
using System.Threading.Tasks;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Common.Utils;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Model.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyGroceries.Inventory.Api;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IMapper _mapper;
    private readonly IMessagingService _messagingService;
    private readonly InventoryContext _inventoryContext;

    public HttpTrigger(IMapper mapper, ILogger<HttpTrigger> logger, IMessagingService messagingService, InventoryContext inventoryContext)
    {
        _mapper = mapper;
        _logger = logger;
        _messagingService = messagingService;
        _inventoryContext = inventoryContext;
    }

    [FunctionName("GetProductsAsync")]
    public async Task<IActionResult> GetProductsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Products")]
        HttpRequest req)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName}", nameof(GetProductsAsync));

        var products = await _inventoryContext.Products.ToListAsync();
        return new OkObjectResult(
            StandardResponse.Success(products));
    }

    [FunctionName("CreateProductAsync")]
    public async Task<IActionResult> CreateProductAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Products")]
        HttpRequest req)
    {
        var createProductRequestDto = await req.GetBody<CreateProductRequestDto>();

        _logger.LogInformation("Processing request for MethodName: {MethodName} with input: {@Input}",
            nameof(CreateProductAsync), createProductRequestDto);

        if (!DtoValidation.IsValid(createProductRequestDto, out var result)) return result;

        var existingProduct = await _inventoryContext.Products.FirstOrDefaultAsync(p =>
            p.Name == createProductRequestDto.Name);
        if (existingProduct != null)
        {
            var errorMessage = "Product with same name already exists. Use a different name.";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        var newProduct = _mapper.Map<Product>(createProductRequestDto);
        newProduct.Id = $"{Constants.ProductPrefix}{Guid.NewGuid()}";
        await _inventoryContext.AddAsync(newProduct);
        await _inventoryContext.SaveChangesAsync();

        var productCreatedEvent = _mapper.Map<ProductCreatedEvent>(newProduct);
        await _messagingService.EmitEventAsync(productCreatedEvent);

        return new CreatedResult(new Uri(req.Path.ToUriComponent()),
            StandardResponse.Success(newProduct));
    }
}