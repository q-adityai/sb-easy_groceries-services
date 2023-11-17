using AutoFixture;
using AutoFixture.AutoMoq;
using Azure.Messaging.ServiceBus;
using EasyGroceries.Common.Messaging;
using EasyGroceries.Common.Messaging.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyGroceries.Common.Tests.Messaging;

public class MessagingServiceTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly MessagingService _service;
    private readonly Mock<ServiceBusClient> _serviceBusClientMock;
    public MessagingServiceTests()
    {
        _serviceBusClientMock = new Mock<ServiceBusClient>();

        _service = new MessagingService(_serviceBusClientMock.Object, new Mock<ILogger<MessagingService>>().Object);
    }

    [Fact]
    public async Task EmitEventAsync_Emits_Event_Successfully()
    {
        //Arrange
        var serviceBusMessageSenderMock = new Mock<ServiceBusSender>();

        _serviceBusClientMock.Setup(x => x.CreateSender(It.IsAny<string>()))
            .Returns(serviceBusMessageSenderMock.Object);
        
        //Act
        var result = _service.EmitEventAsync(_fixture.Create<ProductCreatedEvent>());
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        serviceBusMessageSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task EmitEventsAsync_Emits_EventBatch_Successfully()
    {
        //Arrange
        var serviceBusMessageSenderMock = new Mock<ServiceBusSender>();

        _serviceBusClientMock.Setup(x => x.CreateSender(It.IsAny<string>()))
            .Returns(serviceBusMessageSenderMock.Object);
        
        //Act
        var result = _service.EmitEventsAsync(_fixture.CreateMany<ProductCreatedEvent>().ToList());
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        serviceBusMessageSenderMock.Verify(x => x.SendMessagesAsync(It.IsAny<List<ServiceBusMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task EmitEventsAsync_Does_Not_Emit_Event_When_None_Supplied()
    {
        //Arrange
        var serviceBusMessageSenderMock = new Mock<ServiceBusSender>();

        _serviceBusClientMock.Setup(x => x.CreateSender(It.IsAny<string>()))
            .Returns(serviceBusMessageSenderMock.Object);
        
        //Act
        var result = _service.EmitEventsAsync(_fixture.CreateMany<ProductCreatedEvent>(0).ToList());
        await result;

        //Assert
        result.IsCompletedSuccessfully.Should().BeTrue();
        serviceBusMessageSenderMock.Verify(x => x.SendMessagesAsync(It.IsAny<List<ServiceBusMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}