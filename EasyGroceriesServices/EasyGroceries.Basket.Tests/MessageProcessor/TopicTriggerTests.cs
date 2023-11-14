using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Basket.MessageProcessor;
using EasyGroceries.Basket.Model.Context;
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
        await using var userContext = CreateMockDbContext(nameof(UserSubscriberAsync_Processes_UserCreatedEvent_Successfully));
        var service = new TopicTrigger(new Mock<ILogger<TopicTrigger>>().Object, _mapper, userContext);

        var setupEvent = _fixture.Create<UserCreatedEvent>();

        //Act
        var result = service.UserSubscriberAsync(JsonConvert.SerializeObject(setupEvent));
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        userContext.Users.Count().Should().Be(1);
        userContext.Users.First().Id.Should().Be(setupEvent.Id);
    }
}