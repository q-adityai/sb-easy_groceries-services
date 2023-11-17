using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;
using EasyGroceries.Order.Api;
using EasyGroceries.Order.Dto;
using EasyGroceries.Order.Model.Context;
using EasyGroceries.Order.Model.Entities;
using EasyGroceries.Tests.Common.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyGroceries.Order.Tests.Api;

public class HttpTriggerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private OrderContext CreateMockDbContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<OrderContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new OrderContext(dbOptions.Options);
    }

    [Fact]
    public async Task SubmitOrderAsync_BasketId_Null_Returns_BadRequest()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(SubmitOrderAsync_BasketId_Null_Returns_BadRequest));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, orderContext);

        var request = MockHttpRequest.Create();

        //Act
        var response = await service.SubmitOrderAsync(request.Object, null!) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task SubmitOrderAsync_Order_Not_Found_Returns_NotFound()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(SubmitOrderAsync_Order_Not_Found_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, orderContext);

        var request = MockHttpRequest.Create();

        //Act
        var response = await service.SubmitOrderAsync(request.Object, "b_123") as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task SubmitOrderAsync_User_Not_Found_Returns_NotFound()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(SubmitOrderAsync_Order_Not_Found_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, orderContext);

        var setupBasketId = _fixture.Create<string>();

        await orderContext.Orders.AddAsync(_fixture.Build<Model.Entities.Order>().With(o=> o.BasketId, setupBasketId).Create());
        await orderContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create();

        //Act
        var response = await service.SubmitOrderAsync(request.Object, setupBasketId) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task SubmitOrderAsync_Returns_OrderDetails()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(SubmitOrderAsync_Order_Not_Found_Returns_NotFound));
        var service = new HttpTrigger(new Mock<ILogger<HttpTrigger>>().Object, orderContext);

        var setupBasketId = _fixture.Create<string>();
        var setupOrder = _fixture.Build<Model.Entities.Order>().With(o => o.BasketId, setupBasketId).Create();

        await orderContext.Users.AddAsync(_fixture.Build<User>().With(u => u.Id, setupOrder.UserId).Create());
        await orderContext.Orders.AddAsync(setupOrder);
        await orderContext.OrderItems.AddAsync(_fixture.Build<OrderItem>()
            .With(oi => oi.OrderId, setupOrder.Id)
            .With(oi => oi.Price, new Money() {AmountInMinorUnits = 10})
            .With(oi => oi.DiscountedPrice, new Money() {AmountInMinorUnits = 10})
            .With(oi => oi.Quantity, 5)
            .Create());
        
        await orderContext.OrderItems.AddAsync(_fixture.Build<OrderItem>()
            .With(oi => oi.OrderId, setupOrder.Id)
            .With(oi => oi.Price, new Money() {AmountInMinorUnits = 20})
            .With(oi => oi.DiscountedPrice, new Money() {AmountInMinorUnits = 3})
            .With(oi => oi.Quantity, 5)
            .Create());
        
        await orderContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create();

        //Act
        var response = await service.SubmitOrderAsync(request.Object, setupBasketId) as OkObjectResult;
        var result = (StandardResponse<OrderDto>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
        result.Payload.Should().NotBeNull();
        result.Payload!.BasketValue.AmountInMinorUnits.Should().Be(65);
    }
}