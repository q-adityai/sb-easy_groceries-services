using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Order.MessageProcessor;
using EasyGroceries.Order.Model.Context;
using EasyGroceries.Order.Model.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EasyGroceries.Order.Tests.MessageProcessorTests;

public class TopicTriggerTests
{
    private readonly IMapper _mapper;
    private readonly IFixture _fixture;
    public TopicTriggerTests()
    {
        var amConfiguration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program)));
        _mapper = new Mapper(amConfiguration);
        
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }
    
    private OrderContext CreateMockDbContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<OrderContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new OrderContext(dbOptions.Options);
    }
    
    [Fact]
    public async Task UserSubscriberAsync_Processes_UserCreatedEvent_Successfully()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(UserSubscriberAsync_Processes_UserCreatedEvent_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<UserCreatedEvent>().With(x => x.Type, EventType.UserCreated).Create();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        orderContext.Users.Count().Should().Be(1);
        orderContext.Users.First().Id.Should().Be(setupEvent.Id);
    }
    
    [Fact]
    public async Task UserSubscriberAsync_User_Already_Exists_Exits_No_Error()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(UserSubscriberAsync_User_Already_Exists_Exits_No_Error));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<UserCreatedEvent>().With(x => x.Type, EventType.UserCreated).Create();

        await orderContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupEvent.Id).Create());
        await orderContext.SaveChangesAsync();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
    }
    
    [Fact]
    public async Task UserSubscriberAsync_Invalid_Exits_No_Error()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(UserSubscriberAsync_Invalid_Exits_No_Error));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<ProductCreatedEvent>().With(x => x.Type, EventType.ProductCreated).Create();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
    }
    
    [Fact]
    public async Task UserSubscriberAsync_Processes_UserUpdatedEvent_Existing_User_Successfully()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(UserSubscriberAsync_Processes_UserUpdatedEvent_Existing_User_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<UserUpdatedEvent>().With(x => x.Type, EventType.UserUpdated).Create();

        await orderContext.Users.AddAsync(_fixture.Build<User>().With(u => u.Id, setupEvent.Id).Create());
        await orderContext.SaveChangesAsync();
        
        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        orderContext.Users.Count().Should().Be(1);

        var actualUser = orderContext.Users.First();
        
        actualUser.Id.Should().Be(setupEvent.Id);
        actualUser.FirstName.Should().Be(setupEvent.FirstName);
        actualUser.LastName.Should().Be(setupEvent.LastName);
        actualUser.Email.Should().Be(setupEvent.Email);
        actualUser.DefaultDeliveryAddress.Should().BeEquivalentTo(setupEvent.DefaultDeliveryAddress);
        actualUser.DefaultBillingAddress.Should().BeEquivalentTo(setupEvent.DefaultBillingAddress);
    }
    
    [Fact]
    public async Task UserSubscriberAsync_Processes_UserUpdatedEvent_No_Existing_User_Successfully()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(UserSubscriberAsync_Processes_UserUpdatedEvent_No_Existing_User_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<UserUpdatedEvent>().With(x => x.Type, EventType.UserUpdated).Create();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        orderContext.Users.Count().Should().Be(1);

        var actualUser = orderContext.Users.First();
        
        actualUser.Id.Should().Be(setupEvent.Id);
        actualUser.FirstName.Should().Be(setupEvent.FirstName);
        actualUser.LastName.Should().Be(setupEvent.LastName);
        actualUser.Email.Should().Be(setupEvent.Email);
        actualUser.DefaultDeliveryAddress.Should().BeEquivalentTo(setupEvent.DefaultDeliveryAddress);
        actualUser.DefaultBillingAddress.Should().BeEquivalentTo(setupEvent.DefaultBillingAddress);
    }
    
    [Fact]
    public async Task BasketSubscriberAsync_Invalid_Exits_No_Error()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(BasketSubscriberAsync_Invalid_Exits_No_Error));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<UserUpdatedEvent>().With(x => x.Type, EventType.UserUpdated).Create();

        //Act
        var result = service.BasketSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
    }
    
    [Fact]
    public async Task BasketSubscriberAsync_User_Does_Not_Exist_Throws_Exception()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(BasketSubscriberAsync_User_Does_Not_Exist_Throws_Exception));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<ProductCheckedOutEvent>().With(x => x.Type, EventType.ProductCheckedOut).Create();

        //Act
        var result = await service.Awaiting(x => x.BasketSubscriberAsync(JsonConvert.SerializeObject(setupEvent)))
            .Should().ThrowAsync<Exception>();

        //Assert
        result.Which.Message.Should().Be($"User with id: {setupEvent.UserId} not found");
    }
    
    [Fact]
    public async Task BasketSubscriberAsync_New_Order_And_OrderItem_Created()
    {
        //Arrange
        await using var orderContext = CreateMockDbContext(nameof(BasketSubscriberAsync_New_Order_And_OrderItem_Created));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, orderContext);

        var setupEvent = _fixture.Build<ProductCheckedOutEvent>().With(x => x.Type, EventType.ProductCheckedOut).Create();

        await orderContext.Users.AddAsync(_fixture.Build<User>().With(u => u.Id, setupEvent.UserId).Create());
        await orderContext.SaveChangesAsync();
        
        //Act
        var result = service.BasketSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        var newOrders = await orderContext.Orders.Where(o => o.BasketId == setupEvent.BasketId).ToListAsync();
        newOrders.Count.Should().Be(1);

        var newOrderItems = await orderContext.OrderItems.Where(oi => oi.OrderId == newOrders[0].Id).ToListAsync();
        newOrderItems.Count.Should().Be(1);
    }
}