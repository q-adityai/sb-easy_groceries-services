using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Basket.MessageProcessor;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EasyGroceries.Basket.Tests.MessageProcessor;

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
    
    private BasketContext CreateMockDbContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<BasketContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new BasketContext(dbOptions.Options);
    }

    [Fact]
    public async Task UserSubscriberAsync_Processes_UserCreatedEvent_Successfully()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(UserSubscriberAsync_Processes_UserCreatedEvent_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

        var setupEvent = _fixture.Build<UserCreatedEvent>().With(x => x.Type, EventType.UserCreated).Create();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        basketContext.Users.Count().Should().Be(1);
        basketContext.Users.First().Id.Should().Be(setupEvent.Id);
    }
    
    [Fact]
    public async Task UserSubscriberAsync_User_Already_Exists_Exits_No_Error()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(UserSubscriberAsync_User_Already_Exists_Exits_No_Error));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

        var setupEvent = _fixture.Build<UserCreatedEvent>().With(x => x.Type, EventType.UserCreated).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(x => x.Id, setupEvent.Id).Create());
        await basketContext.SaveChangesAsync();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
    }
    
    [Fact]
    public async Task UserSubscriberAsync_Invalid_Event_Exits_No_Error()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(UserSubscriberAsync_Invalid_Event_Exits_No_Error));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

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
        await using var basketContext = CreateMockDbContext(nameof(UserSubscriberAsync_Processes_UserUpdatedEvent_Existing_User_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

        var setupEvent = _fixture.Build<UserUpdatedEvent>().With(x => x.Type, EventType.UserUpdated).Create();

        await basketContext.Users.AddAsync(_fixture.Build<User>().With(u => u.Id, setupEvent.Id).Create());
        await basketContext.SaveChangesAsync();
        
        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        basketContext.Users.Count().Should().Be(1);

        var actualUser = basketContext.Users.First();
        
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
        await using var basketContext = CreateMockDbContext(nameof(UserSubscriberAsync_Processes_UserUpdatedEvent_No_Existing_User_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

        var setupEvent = _fixture.Build<UserUpdatedEvent>().With(x => x.Type, EventType.UserUpdated).Create();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        basketContext.Users.Count().Should().Be(1);

        var actualUser = basketContext.Users.First();
        
        actualUser.Id.Should().Be(setupEvent.Id);
        actualUser.FirstName.Should().Be(setupEvent.FirstName);
        actualUser.LastName.Should().Be(setupEvent.LastName);
        actualUser.Email.Should().Be(setupEvent.Email);
        actualUser.DefaultDeliveryAddress.Should().BeEquivalentTo(setupEvent.DefaultDeliveryAddress);
        actualUser.DefaultBillingAddress.Should().BeEquivalentTo(setupEvent.DefaultBillingAddress);
    }
    
    [Fact]
    public async Task ProductSubscriberAsync_Processes_ProductCreatedEvent_Successfully()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(ProductSubscriberAsync_Processes_ProductCreatedEvent_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

        var setupEvent = _fixture.Build<ProductCreatedEvent>().With(x => x.Type, EventType.ProductCreated).Create();

        //Act
        var result = service.InventorySubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        basketContext.Products.Count().Should().Be(1);
        basketContext.Products.First().Id.Should().Be(setupEvent.Id);
    }
    
    [Fact]
    public async Task InventorySubscriberAsync_Invalid_Exits_No_Error()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(InventorySubscriberAsync_Invalid_Exits_No_Error));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

        var setupEvent = _fixture.Build<UserUpdatedEvent>().With(x => x.Type, EventType.UserUpdated).Create();

        //Act
        var result = service.InventorySubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
    }
    
    [Fact]
    public async Task InventorySubscriberAsync_Product_already_Exists_Exits_No_Error()
    {
        //Arrange
        await using var basketContext = CreateMockDbContext(nameof(InventorySubscriberAsync_Product_already_Exists_Exits_No_Error));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, basketContext);

        var setupEvent = _fixture.Build<ProductCreatedEvent>().With(x => x.Type, EventType.ProductCreated).Create();

        await basketContext.Products.AddAsync(_fixture.Build<Product>().With(x => x.Id, setupEvent.Id).Create());
        await basketContext.SaveChangesAsync();

        //Act
        var result = service.InventorySubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
    }
}