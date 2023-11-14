using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Tests.Common.Utils;
using EasyGroceries.User.Api;
using EasyGroceries.User.Dto;
using EasyGroceries.User.Model.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EasyGroceries.User.Tests.Api;

public class ApiTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IMessagingService> _messagingServiceMock;
    private readonly IMapper _mapper;
    public ApiTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        var amConfiguration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program)));
        _mapper = new Mapper(amConfiguration);
        _messagingServiceMock = new Mock<IMessagingService>();
    }
    
    private UserContext CreateMockDbContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<UserContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new UserContext(dbOptions.Options);
    }

    [Fact]
    public async Task GetUsers_Returns_No_Users()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(GetUsers_Returns_No_Users));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var request = MockHttpRequest.Create();

        //Act
        var response = await service.GetUsersAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<Model.Entities.User>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetUsers_Returns_Users()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(GetUsers_Returns_Users));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var setupUsers = _fixture.CreateMany<Model.Entities.User>(5).ToList();
        await userContext.AddRangeAsync(setupUsers);
        await userContext.SaveChangesAsync();
        
        var request = MockHttpRequest.Create();

        //Act
        var response = await service.GetUsersAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<Model.Entities.User>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeEmpty();
        result.Payload!.Count.Should().Be(5);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetUser_Not_Found()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(GetUser_Not_Found));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        var request = MockHttpRequest.Create();

        //Act
        var response = await service.GetUserAsync(request.Object, "u_111") as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x == "User with userId: u_111 not found");
    }


    [Fact]
    public async Task GetUser_Returns_User()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(GetUser_Returns_User));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var setupUser = _fixture.Build<Model.Entities.User>()
            .With(x => x.Id, "u_111")
            .With(x => x.IsActive, true)
            .Create();

        await userContext.AddAsync(setupUser);
        await userContext.SaveChangesAsync();

        var request = MockHttpRequest.Create();

        //Act
        var response = await service.GetUserAsync(request.Object, "u_111") as OkObjectResult;
        var result = (StandardResponse<Model.Entities.User>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Id.Should().Be("u_111");
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task CreateUser_Valid_Input_Successfully_Creates_User()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(CreateUser_Valid_Input_Successfully_Creates_User));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var inputUser = new CreateUserRequestDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "mail@example.com",
            DefaultAddress = new DefaultAddress
            {
                Line1 = "Some line 1",
                Postcode = "AAA BBC",
                CountryCode = CountryCode.Gb
            }
        };

        var request = MockHttpRequest.Create("/example.com", body: inputUser);

        //Act
        var response = await service.CreateUserAsync(request.Object) as CreatedResult;
        var result = (StandardResponse<Model.Entities.User>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        result.Payload.Should().NotBeNull();
        result.Payload!.FirstName.Should().Be("Test");
        result.Payload!.LastName.Should().Be("User");
        result.Payload!.Email.Should().Be("mail@example.com");
        result.Payload!.DefaultBillingAddress!.Line1.Should().Be("Some line 1");
        result.Payload!.DefaultBillingAddress!.Postcode.Should().Be("AAA BBC");
        result.Payload!.DefaultBillingAddress!.CountryCode.Should().Be(CountryCode.Gb);
        result.Payload!.DefaultDeliveryAddress!.Line1.Should().Be("Some line 1");
        result.Payload!.DefaultDeliveryAddress!.Postcode.Should().Be("AAA BBC");
        result.Payload!.DefaultDeliveryAddress!.CountryCode.Should().Be(CountryCode.Gb);
        (await userContext.Addresses.FirstOrDefaultAsync())!.Line1.Should().Be("Some line 1");
        result.Payload!.IsActive.Should().BeTrue();
        result.Payload!.IsAdmin.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();

        _messagingServiceMock.Verify(x => x.EmitEventAsync(It.Is<IEvent>(e => e.Type == EventType.UserCreated)), Times.Once);
    }

    [Theory]
    [InlineData(null!, null!, null!)]
    [InlineData("", "", "")]
    [InlineData("Test", "", "")]
    [InlineData("Test", "User", "")]
    [InlineData("Test", "", "email@example.com")]
    [InlineData("", "User", "email@example.com")]
    [InlineData("", "", "email@example.com")]
    [InlineData("", "User", "")]
    public async Task CreateUser_InValid_Input_Returns_BadRequest(string firstName, string lastName, string email)
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(CreateUser_InValid_Input_Returns_BadRequest));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var inputUser = new CreateUserRequestDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        var request = MockHttpRequest.Create("/example.com", body: inputUser);


        //Act
        var response = await service.CreateUserAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUser_Valid_Input_Already_Existing_User()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(CreateUser_InValid_Input_Returns_BadRequest));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var inputUser = new CreateUserRequestDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "mail@example.com"
        };

        await userContext.AddAsync(_fixture.Build<Model.Entities.User>().With(x => x.Email, "mail@example.com").Create());
        await userContext.SaveChangesAsync();


        var request = MockHttpRequest.Create("/example.com", body: inputUser);

        

        //Act
        var response = await service.CreateUserAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task UpdateUserAddressAsync_Valid_Input_Existing_User_Updates_User()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(UpdateUserAddressAsync_Valid_Input_Existing_User_Updates_User));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var inputUser = new UpdateUserAddressRequestDto
        {
            UserId = "u_123",
            Address = new DefaultAddress
            {
                Line1 = "Changed line 1",
                Line2 = "Added line 2",
                Postcode = "XXYY 123",
                CountryCode = CountryCode.Gb
            }
        };

        await userContext.AddAsync(_fixture.Build<Model.Entities.User>().With(x => x.Id, "u_123").Create());
        await userContext.SaveChangesAsync();

        var request = MockHttpRequest.Create(body: inputUser);

        //Act
        var response = await service.UpdateUserAddressAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<Model.Entities.User>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.DefaultBillingAddress!.Line1.Should().Be("Changed line 1");
        result.Payload!.DefaultBillingAddress!.Line2.Should().Be("Added line 2");
        result.Payload!.DefaultBillingAddress!.Postcode.Should().Be("XXYY 123");
        result.Payload!.DefaultDeliveryAddress!.Line1.Should().Be("Changed line 1");
        result.Payload!.DefaultDeliveryAddress!.Line2.Should().Be("Added line 2");
        result.Payload!.DefaultDeliveryAddress!.Postcode.Should().Be("XXYY 123");
        (await userContext.Addresses.FirstOrDefaultAsync())!.Line1.Should().Be("Changed line 1");
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }
    
    [Fact]
    public async Task UpdateUserAddressAsync_Valid_Input_No_Existing_User_Returns_NotFound()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(UpdateUserAddressAsync_Valid_Input_No_Existing_User_Returns_NotFound));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var inputUser = new UpdateUserAddressRequestDto
        {
            UserId = "u_123",
            Address = new DefaultAddress
            {
                Line1 = "Changed line 1",
                Line2 = "Added line 2",
                Postcode = "XXYY 123",
                CountryCode = CountryCode.Gb
            }
        };

        var request = MockHttpRequest.Create(body: inputUser);

        //Act
        var response = await service.UpdateUserAddressAsync(request.Object) as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task UpdateUserAddressAsync_Invalid_Input_Returns_BadRequest()
    {
        //Arrange
        await using var userContext = CreateMockDbContext(nameof(UpdateUserAddressAsync_Valid_Input_No_Existing_User_Returns_NotFound));
        var service = new HttpTrigger(_mapper, _messagingServiceMock.Object, new Mock<ILogger<HttpTrigger>>().Object,
            userContext);
        
        var inputUser = new UpdateUserAddressRequestDto
        {
            UserId = "u_123",
        };

        var request = MockHttpRequest.Create(body: inputUser);

        //Act
        var response = await service.UpdateUserAddressAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}