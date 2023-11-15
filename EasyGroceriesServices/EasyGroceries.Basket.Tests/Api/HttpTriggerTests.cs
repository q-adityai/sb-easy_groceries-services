using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Basket.Api;
using EasyGroceries.Basket.Configuration;
using EasyGroceries.Basket.Dto;
using EasyGroceries.Basket.MessageProcessor;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Tests.Common.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EasyGroceries.Basket.Tests.Api;

public class HttpTriggerTests
{
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;
    private readonly Mock<IOptions<BasketApiOptions>> _options;
    private readonly Mock<IMessagingService> _messagingServiceMock;
    public HttpTriggerTests()
    {
        var amConfiguration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program)));
        _mapper = new Mapper(amConfiguration);

        _options = new Mock<IOptions<BasketApiOptions>>();
        _options.Setup(x => x.Value)
            .Returns(new BasketApiOptions
            {
                DefaultDiscountPercentInMinorUnits = 2000
            });
        
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _messagingServiceMock = new Mock<IMessagingService>();
    }
    
    private BasketContext CreateMockDbContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<BasketContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new BasketContext(dbOptions.Options);
    }

    [Theory]
    [InlineData("", "", 0)]
    [InlineData("", "111", 0)]
    [InlineData("111", "", 0)]
    public async Task AddProductToBasketAsync_Invalid_Input_Returns_BadRequest(string userId, string productId, long quantity)
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Invalid_Input_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>()
            .With(x => x.UserId, userId)
            .With(x => x.ProductId, productId)
            .With(x => x.Quantity, quantity)
            .Create();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_User_Does_not_exist_Returns_BadRequest()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_User_Does_not_exist_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_Product_Does_not_exist_Returns_BadRequest()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_Product_Does_not_exist_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_Discount_Multi_Add_Returns_BadRequest()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_Discount_Multi_Add_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).With(x => x.Category, ProductCategory.PromotionCoupon).Create());
        
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_Returns_Success()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_Returns_Success));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Without(x => x.BasketId).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).With(x => x.Category, ProductCategory.Dairy).Create());
        
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.Basket>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Status.Should().Be(BasketStatus.Active);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_Basket_Not_Found_Returns_NotFound()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_Basket_Not_Found_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).With(x => x.Category, ProductCategory.Dairy).Create());
        
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_Basket_Does_Not_Belong_to_User_Returns_NotFound()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_Basket_Does_Not_Belong_to_User_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).With(x => x.Category, ProductCategory.Dairy).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, "123").Create());
        
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_Basket_Already_Contains_PromotionCoupon_Returns_BadRequest()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_Basket_Already_Contains_PromotionCoupon_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).With(x => x.Category, ProductCategory.Dairy).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, setupRequest.UserId).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<BasketProduct>().With(x => x.BasketId, setupRequest.BasketId).With(x => x.ProductId, setupRequest.ProductId).With(x => x.Category, ProductCategory.PromotionCoupon).Create());
        
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AddProductToBasketAsync_Valid_Input_Basket_Already_Contains_Product_Returns_Increases_Quantity()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(AddProductToBasketAsync_Valid_Input_Basket_Already_Contains_Product_Returns_Increases_Quantity));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).With(x => x.Category, ProductCategory.Dairy).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, setupRequest.UserId).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<BasketProduct>().With(x => x.BasketId, setupRequest.BasketId).With(x => x.ProductId, setupRequest.ProductId).With(x => x.Quantity, 5).Create());
        
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.AddProductToBasketAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.Basket>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Status.Should().Be(BasketStatus.Active);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
        (await basketContext.BasketProducts.FirstOrDefaultAsync())!.Quantity.Should().Be(15);
    }
    
    
    [Theory]
    [InlineData("", "", 0, "")]
    [InlineData("", "111", 0, "")]
    [InlineData("111", "", 0, "")]
    [InlineData("111", "222", 10, "")]
    public async Task RemoveProductFromBasketAsync_Invalid_Input_Returns_BadRequest(string userId, string productId, long quantity, string basketId)
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Invalid_Input_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>()
            .With(x => x.BasketId, basketId)
            .With(x => x.UserId, userId)
            .With(x => x.ProductId, productId)
            .With(x => x.Quantity, quantity)
            .Create();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_User_Does_not_exist_Returns_NotFound()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_User_Does_not_exist_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Product_Does_not_exist_Returns_NotFound()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Product_Does_not_exist_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Basket_Does_not_exist_Returns_NotFound()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Basket_Does_not_exist_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Basket_Does_belong_to_user_Returns_NotFound()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Basket_Does_belong_to_user_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, "123").Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Intended_Product_Not_Found_Returns_BadRequest()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Intended_Product_Not_Found_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, setupRequest.UserId).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Higher_quantity_To_be_Removed_Returns_BadRequest()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Intended_Product_Not_Found_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, setupRequest.UserId).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<Model.Entities.BasketProduct>().With(x => x.BasketId, setupRequest.BasketId).With(x => x.ProductId, setupRequest.ProductId).With(x => x.Quantity, 2).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Same_quantity_To_be_Removed_Returns_Success()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Intended_Product_Not_Found_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, setupRequest.UserId).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<Model.Entities.BasketProduct>().With(x => x.BasketId, setupRequest.BasketId).With(x => x.ProductId, setupRequest.ProductId).With(x => x.Quantity, setupRequest.Quantity).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<Model.Entities.BasketProduct>().With(x => x.BasketId, setupRequest.BasketId).With(x => x.Quantity, 5).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.Basket>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Status.Should().Be(BasketStatus.Active);
        (await basketContext.BasketProducts.CountAsync(bp =>
            bp.BasketId == setupRequest.BasketId && bp.ProductId == setupRequest.ProductId)).Should().Be(0);
        (await basketContext.BasketProducts.CountAsync()).Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Same_quantity_To_be_Removed_Last_Product_Returns_Success()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Same_quantity_To_be_Removed_Last_Product_Returns_Success));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, setupRequest.UserId).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<Model.Entities.BasketProduct>().With(x => x.BasketId, setupRequest.BasketId).With(x => x.ProductId, setupRequest.ProductId).With(x => x.Quantity, setupRequest.Quantity).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.Basket>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Status.Should().Be(BasketStatus.Empty);
        (await basketContext.BasketProducts.CountAsync(bp =>
            bp.BasketId == setupRequest.BasketId && bp.ProductId == setupRequest.ProductId)).Should().Be(0);
        (await basketContext.BasketProducts.CountAsync()).Should().Be(0);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }
    
    [Fact]
    public async Task RemoveProductFromBasketAsync_Valid_Input_Lesser_quantity_To_be_Removed_Returns_Success()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(RemoveProductFromBasketAsync_Valid_Input_Intended_Product_Not_Found_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupRequest = _fixture.Build<BasketProductRequestDto>().With(x => x.Quantity, 10).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupRequest.UserId).Create());
        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupRequest.ProductId).Create());
        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupRequest.BasketId).With(x => x.UserId, setupRequest.UserId).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<Model.Entities.BasketProduct>().With(x => x.BasketId, setupRequest.BasketId).With(x => x.ProductId, setupRequest.ProductId).With(x => x.Quantity, 50).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create(body: setupRequest);

        //Act
        var response = await service.RemoveProductFromBasketAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.Basket>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Status.Should().Be(BasketStatus.Active);
        (await basketContext.BasketProducts.CountAsync(bp =>
            bp.BasketId == setupRequest.BasketId && bp.ProductId == setupRequest.ProductId)).Should().Be(1);
        (await basketContext.BasketProducts.FirstOrDefaultAsync(bp =>
            bp.BasketId == setupRequest.BasketId && bp.ProductId == setupRequest.ProductId))!.Quantity.Should().Be(40);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task CheckoutBasketAsync_No_BasketId_Returns_Bad_Request()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(CheckoutBasketAsync_No_BasketId_Returns_Bad_Request));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);
        
        var request = MockHttpRequest.Create();
        
        //Act
        var response = await service.CheckoutBasketAsync(request.Object, null!) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CheckoutBasketAsync_Basket_Not_Found_Returns_Not_Found()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(CheckoutBasketAsync_No_BasketId_Returns_Bad_Request));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);
        
        var request = MockHttpRequest.Create();
        
        //Act
        var response = await service.CheckoutBasketAsync(request.Object, _fixture.Create<string>()) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CheckoutBasketAsync_Basket_Not_Active_Returns_Bad_Request()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(CheckoutBasketAsync_Basket_Not_Active_Returns_Bad_Request));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);
        
        var request = MockHttpRequest.Create();
        var setupBasketId = _fixture.Create<string>();

        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupBasketId).With(x => x.Status, BasketStatus.Empty).Create());
        await basketContext.SaveChangesAsync();
        
        //Act
        var response = await service.CheckoutBasketAsync(request.Object, setupBasketId) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CheckoutBasketAsync_Basket_No_Promotions_Returns_Success()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(CheckoutBasketAsync_Basket_No_Promotions_Returns_Success));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);
        
        var request = MockHttpRequest.Create();
        var setupBasketId = _fixture.Create<string>();

        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupBasketId).With(x => x.Status, BasketStatus.Active).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<BasketProduct>()
            .With(x => x.BasketId,
                setupBasketId)
            .With(x => x.Category,
                ProductCategory.Dairy)
            .With(x => x.Quantity,
                2)
            .With(x => x.Price,
                new Money()
                {
                    AmountInMinorUnits = 10
                })
            .With(x => x.DiscountedPrice,
                new Money()
                {
                    AmountInMinorUnits = 10
                })
            .With(x => x.DiscountPercentInMinorUnits,0)
            .Create());
        
        await basketContext.BasketProducts.AddAsync(_fixture.Build<BasketProduct>()
            .With(x => x.BasketId,
                setupBasketId)
            .With(x => x.Category,
                ProductCategory.Breads)
            .With(x => x.Quantity,
                5)
            .With(x => x.Price,
                new Money()
                {
                    AmountInMinorUnits = 15
                })
            .With(x => x.DiscountedPrice,
                new Money()
                {
                    AmountInMinorUnits = 15
                })
            .With(x => x.DiscountPercentInMinorUnits,0)
            .Create());
        await basketContext.SaveChangesAsync();
        
        //Act
        var response = await service.CheckoutBasketAsync(request.Object, setupBasketId) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.Basket>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Status.Should().Be(BasketStatus.CheckedOut);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
        (await basketContext.BasketProducts.CountAsync(bp =>
            bp.BasketId == setupBasketId && bp.Category == ProductCategory.Dairy &&
            bp.DiscountedPrice.AmountInMinorUnits == 10 && bp.DiscountPercentInMinorUnits == 0)).Should().Be(1);
        (await basketContext.BasketProducts.CountAsync(bp =>
            bp.BasketId == setupBasketId && bp.Category == ProductCategory.Breads &&
            bp.DiscountedPrice.AmountInMinorUnits == 15 && bp.DiscountPercentInMinorUnits == 0)).Should().Be(1);
        
        _messagingServiceMock.Verify(x => x.EmitEventsAsync(It.Is<List<ProductCheckedOutEvent>>(l => l.Count == 2)), Times.Once);
    }
    
    [Fact]
    public async Task CheckoutBasketAsync_Basket_Promotion_Present_Returns_Success()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(CheckoutBasketAsync_Basket_No_Promotions_Returns_Success));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);
        
        var request = MockHttpRequest.Create();
        var setupBasketId = _fixture.Create<string>();

        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupBasketId).With(x => x.Status, BasketStatus.Active).Create());
        await basketContext.BasketProducts.AddAsync(_fixture.Build<BasketProduct>()
            .With(x => x.BasketId,
                setupBasketId)
            .With(x => x.Category,
                ProductCategory.Dairy)
            .With(x => x.Quantity,
                2)
            .With(x => x.Price,
                new Money()
                {
                    AmountInMinorUnits = 10
                })
            .With(x => x.DiscountedPrice,
                new Money()
                {
                    AmountInMinorUnits = 10
                })
            .With(x => x.DiscountPercentInMinorUnits,0)
            .With(x => x.DiscountApplicable, true)
            .Create());
        
        await basketContext.BasketProducts.AddAsync(_fixture.Build<BasketProduct>()
            .With(x => x.BasketId,
                setupBasketId)
            .With(x => x.Category,
                ProductCategory.Breads)
            .With(x => x.Quantity,
                5)
            .With(x => x.Price,
                new Money()
                {
                    AmountInMinorUnits = 15
                })
            .With(x => x.DiscountedPrice,
                new Money()
                {
                    AmountInMinorUnits = 15
                })
            .With(x => x.DiscountPercentInMinorUnits,0)
            .With(x => x.DiscountApplicable, true)
            .Create());
        
        await basketContext.BasketProducts.AddAsync(_fixture.Build<BasketProduct>()
            .With(x => x.BasketId,
                setupBasketId)
            .With(x => x.Category,
                ProductCategory.PromotionCoupon)
            .With(x => x.Quantity,
                1)
            .With(x => x.Price,
                new Money()
                {
                    AmountInMinorUnits = 5
                })
            .With(x => x.DiscountedPrice,
                new Money()
                {
                    AmountInMinorUnits = 5
                })
            .With(x => x.DiscountPercentInMinorUnits,0)
            .With(x => x.DiscountApplicable, false)
            .Create());
        await basketContext.SaveChangesAsync();
        
        //Act
        var response = await service.CheckoutBasketAsync(request.Object, setupBasketId) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.Basket>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Status.Should().Be(BasketStatus.CheckedOut);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
        (await basketContext.BasketProducts.CountAsync(bp =>
            bp.BasketId == setupBasketId && bp.Category == ProductCategory.Dairy &&
            bp.DiscountedPrice.AmountInMinorUnits == 8 && bp.DiscountPercentInMinorUnits == 2000)).Should().Be(1);
        (await basketContext.BasketProducts.CountAsync(bp =>
            bp.BasketId == setupBasketId && bp.Category == ProductCategory.Breads &&
            bp.DiscountedPrice.AmountInMinorUnits == 12 && bp.DiscountPercentInMinorUnits == 2000)).Should().Be(1);
        
        _messagingServiceMock.Verify(x => x.EmitEventsAsync(It.Is<List<ProductCheckedOutEvent>>(l => l.Count == 3)), Times.Once);
    }
    
    [Fact]
    public async Task BasketPreviewAsync_No_BasketId_Returns_Bad_Request()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(BasketPreviewAsync_No_BasketId_Returns_Bad_Request));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);
        
        var request = MockHttpRequest.Create();
        
        //Act
        var response = await service.BasketPreviewAsync(request.Object, null!) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task BasketPreviewAsync_Basket_Not_Found_Returns_Not_Found()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(BasketPreviewAsync_Basket_Not_Found_Returns_Not_Found));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);
        
        var request = MockHttpRequest.Create();
        
        //Act
        var response = await service.BasketPreviewAsync(request.Object, _fixture.Create<string>()) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task BasketPreviewAsync_User_Not_Found_Returns_Not_Found()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(BasketPreviewAsync_User_Not_Found_Returns_Not_Found));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupBasketId = _fixture.Create<string>();

        await basketContext.Baskets.AddAsync(_fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupBasketId).Create());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create();
        
        //Act
        var response = await service.BasketPreviewAsync(request.Object, setupBasketId) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task BasketPreviewAsync_Returns_Success()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(BasketPreviewAsync_Returns_Success));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, _messagingServiceMock.Object,
            basketContext, _options.Object);

        var setupBasketId = _fixture.Create<string>();
        var setupUser = _fixture.Create<User>();
        var setupBasket = _fixture.Build<Model.Entities.Basket>().With(x => x.Id, setupBasketId).With(x => x.UserId, setupUser.Id).Create();

        await basketContext.Baskets.AddAsync(setupBasket);
        await basketContext.Users.AddAsync(setupUser);
        await basketContext.BasketProducts.AddRangeAsync(_fixture.Build<BasketProduct>().With(x => x.BasketId, setupBasketId).CreateMany());
        await basketContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create();
        
        //Act
        var response = await service.BasketPreviewAsync(request.Object, setupBasketId) as OkObjectResult;
        var result = (StandardResponse<BasketPreviewDto>)response!.Value;
        
        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.BasketId.Should().Be(setupBasketId);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }
}