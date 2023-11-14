using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Inventory.Api;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Model.Entities;
using EasyGroceries.Inventory.Repositories.Interfaces;
using EasyGroceries.Tests.Common.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyGroceries.Inventory.Tests.Api;

public class ApiTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IMessagingService> _messagingServiceMock;


    private readonly HttpTrigger _service;

    private readonly Mock<IProductRepository> _productRepositoryMock;

    public ApiTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        var amConfiguration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program)));
        IMapper mapper = new Mapper(amConfiguration);

        _productRepositoryMock = new Mock<IProductRepository>();
        _messagingServiceMock = new Mock<IMessagingService>();

        _service = new HttpTrigger(mapper, new Mock<ILogger<HttpTrigger>>().Object, _productRepositoryMock.Object,
            _messagingServiceMock.Object);
    }

    [Fact]
    public async Task GetApplicableProductsAsync_Returns_Products()
    {
        //Arrange
        var setupProducts = _fixture.CreateMany<Product>().ToList();
        var request = MockHttpRequest.Create();
        _productRepositoryMock.Setup(x => x.GetAllApplicableProductsAsync())
            .ReturnsAsync(setupProducts);

        //Act
        var response = await _service.GetApplicableProductsAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<ProductDto>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeEmpty();
        result.Payload!.Count.Should().Be(setupProducts.Count);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsAsync_Returns_Products()
    {
        //Arrange
        var setupProducts = _fixture.CreateMany<Product>().ToList();
        var request = MockHttpRequest.Create();
        _productRepositoryMock.Setup(x => x.GetProductsAsync())
            .ReturnsAsync(setupProducts);

        //Act
        var response = await _service.GetProductsAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<ProductDto>>)response!.Value;

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
        var request = MockHttpRequest.Create(path: "/example.com",
            body: _fixture.Build<ProductDto>().With(x => x.StockQuantity, 5).Create());

        _productRepositoryMock.Setup(x => x.CreateProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(_fixture.Create<Product>());

        //Act
        var response = await _service.CreateProductAsync(request.Object) as CreatedResult;
        var result = (StandardResponse<ProductDto>)response!.Value;

        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
        _messagingServiceMock.Verify(x => x.EmitEventAsync(It.Is<ProductCreatedEvent>(x => x.Id == result.Payload!.Id)),
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_Valid_Input_With_Existing_Returns_BadRequest()
    {
        //Arrange
        var request = MockHttpRequest.Create(path: "/example.com",
            body: _fixture.Build<ProductDto>().With(x => x.StockQuantity, 5).Create());

        _productRepositoryMock.Setup(x => x.GetProductByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Product>());

        //Act
        var response = await _service.CreateProductAsync(request.Object) as BadRequestObjectResult;
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
        var request = MockHttpRequest.Create(
            path: "/example.com",
            body: _fixture.Build<ProductDto>()
                .With(x => x.StockQuantity, 5)
                .With(x => x.Name, name)
                .With(x => x.Description, description)
                .Create());

        //Act
        var response = await _service.CreateProductAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}