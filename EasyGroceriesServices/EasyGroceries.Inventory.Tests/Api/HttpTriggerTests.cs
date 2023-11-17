using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Inventory.Api;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Model.Entities;
using EasyGroceries.Tests.Common.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyGroceries.Inventory.Tests.Api;

public class HttpTriggerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IMessagingService> _messagingServiceMock;
    private readonly IMapper _mapper;

    public HttpTriggerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        var amConfiguration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program)));
        _mapper = new Mapper(amConfiguration);

        _messagingServiceMock = new Mock<IMessagingService>();
    }
    
    private InventoryContext CreateMockDbContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<InventoryContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new InventoryContext(dbOptions.Options);
    }

    [Fact]
    public async Task GetProductsAsync_Returns_Products()
    {
        //Arrange
        await using var inventoryContext = CreateMockDbContext(nameof(GetProductsAsync_Returns_Products));
        var service = new HttpTrigger(_mapper, new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object, inventoryContext);
        
        var setupProducts = _fixture.CreateMany<Product>().ToList();
        await inventoryContext.AddRangeAsync(setupProducts);
        await inventoryContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create();

        //Act
        var response = await service.GetProductsAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<Product>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeEmpty();
        result.Payload!.Count.Should().Be(setupProducts.Count);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task CreateProductAsync_Valid_Input_No_Existing_Creates_Product()
    {
        //Arrange
        await using var inventoryContext = CreateMockDbContext(nameof(CreateProductAsync_Valid_Input_No_Existing_Creates_Product));
        var service = new HttpTrigger(_mapper, new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object, inventoryContext);
        
        var request = MockHttpRequest.Create(path: "/example.com",
            body: _fixture.Build<CreateProductRequestDto>().With(x => x.StockQuantity, 5).Create());

        //Act
        var response = await service.CreateProductAsync(request.Object) as CreatedResult;
        var result = (StandardResponse<Product>)response!.Value;

        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
        _messagingServiceMock.Verify(x => x.EmitEventAsync(It.Is<ProductCreatedEvent>(e => e.Id == result.Payload!.Id)),
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_Valid_Input_With_Existing_Returns_BadRequest()
    {
        //Arrange
        await using var inventoryContext = CreateMockDbContext(nameof(CreateProductAsync_Valid_Input_With_Existing_Returns_BadRequest));
        var service = new HttpTrigger(_mapper, new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object, inventoryContext);

        await inventoryContext.AddAsync(_fixture.Build<Product>().With(x => x.Name, "test product").Create());
        await inventoryContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(path: "/example.com",
            body: _fixture.Build<CreateProductRequestDto>().With(x => x.Name, "test product").With(x => x.StockQuantity, 5).Create());

        //Act
        var response = await service.CreateProductAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Name", "")]
    [InlineData("", "Description")]
    public async Task CreateProductAsync_InValid_Input_Returns_BadRequest(string name, string description)
    {
        //Arrange
        await using var inventoryContext = CreateMockDbContext(nameof(CreateProductAsync_InValid_Input_Returns_BadRequest));
        var service = new HttpTrigger(_mapper, new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object, inventoryContext);
        
        var request = MockHttpRequest.Create(
            path: "/example.com",
            body: _fixture.Build<CreateProductRequestDto>()
                .With(x => x.StockQuantity, 5)
                .With(x => x.Name, name)
                .With(x => x.Description, description)
                .Create());

        //Act
        var response = await service.CreateProductAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}