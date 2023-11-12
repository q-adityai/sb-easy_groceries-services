using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyGroceries.Inventory.Api;

public class HttpTrigger
{
    private readonly IMapper _mapper;
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public HttpTrigger(IMapper mapper, ILogger<HttpTrigger> logger, IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }
    
    [FunctionName("GetApplicableProductsAsync")]
    public async Task<IActionResult> GetApplicableProductsAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ApplicableProducts")] HttpRequest req)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName}", nameof(GetApplicableProductsAsync));

        return new OkObjectResult(StandardResponse.Success(_mapper.Map<List<ProductDto>>(await _productRepository.GetAllApplicableProducts())));
    }
}