using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Tests.Common.Utils;
using EasyGroceries.User.Api;
using EasyGroceries.User.Dto;
using EasyGroceries.User.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace EasyGroceries.User.Tests.Api;

public class ApiTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IMessagingService> _messagingServiceMock;


    private readonly HttpTrigger _service;

    private readonly Mock<IUserRepository> _userRepositoryMock;

    public ApiTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        var amConfiguration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program)));
        IMapper mapper = new Mapper(amConfiguration);

        _userRepositoryMock = new Mock<IUserRepository>();
        _messagingServiceMock = new Mock<IMessagingService>();

        _service = new HttpTrigger(mapper, _userRepositoryMock.Object, _messagingServiceMock.Object,
            new Mock<ILogger<HttpTrigger>>().Object);
    }

    [Fact]
    public async Task GetUsers_No_Deleted_Users_Included_Returns_No_Users()
    {
        //Arrange
        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.GetUsersAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<UserDto>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetUsers_Deleted_Users_Included_Returns_No_Users()
    {
        //Arrange
        var queryCollectionItems = new Dictionary<string, StringValues> { { "IncludeDeletedUsers", "True" } };

        var request = new Mock<HttpRequest>();
        request.Setup(x => x.Query).Returns(new QueryCollection(queryCollectionItems));

        //Act
        var response = await _service.GetUsersAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<UserDto>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetUsers_No_Deleted_Users_Included_Returns_Users()
    {
        //Arrange
        var setupUsers = _fixture.Build<Model.Entities.User>()
            .With(x => x.IsActive, true)
            .CreateMany(5)
            .ToList();
        _userRepositoryMock.Setup(x => x.GetUsersAsync(false))
            .ReturnsAsync(setupUsers);


        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.GetUsersAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<UserDto>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeEmpty();
        result.Payload!.Count.Should().Be(setupUsers.Count);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetUsers_Deleted_Users_Included_Returns_Users()
    {
        //Arrange
        var setupUsers = new List<Model.Entities.User>();

        var activeUsers = _fixture.Build<Model.Entities.User>()
            .With(x => x.IsActive, true)
            .CreateMany(5)
            .ToList();

        var inActiveUsers = _fixture.Build<Model.Entities.User>()
            .With(x => x.IsActive, false)
            .CreateMany(8)
            .ToList();

        setupUsers.AddRange(activeUsers);
        setupUsers.AddRange(inActiveUsers);

        _userRepositoryMock.Setup(x => x.GetUsersAsync(true))
            .ReturnsAsync(setupUsers);


        var queryCollectionItems = new Dictionary<string, StringValues> { { "IncludeDeletedUsers", "True" } };

        var request = new Mock<HttpRequest>();
        request.Setup(x => x.Query).Returns(new QueryCollection(queryCollectionItems));

        //Act
        var response = await _service.GetUsersAsync(request.Object) as OkObjectResult;
        var result = (StandardResponse<List<UserDto>>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload!.Count.Should().Be(setupUsers.Count);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetUser_Not_Found()
    {
        //Arrange
        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.GetUserAsync(request.Object, "u_111") as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x == "User with userId: u_111 not found");
    }

    [Fact]
    public async Task GetUser_Include_Deleted_User_Not_Found()
    {
        //Arrange
        var queryCollectionItems = new Dictionary<string, StringValues> { { "IncludeDeletedUsers", "True" } };

        var request = new Mock<HttpRequest>();
        request.Setup(x => x.Query).Returns(new QueryCollection(queryCollectionItems));

        //Act
        var response = await _service.GetUserAsync(request.Object, "u_111") as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x == "User with userId: u_111 not found");
    }

    [Fact]
    public async Task GetUser_Not_Included_Deleted_User_Returns_User()
    {
        //Arrange
        var setupUser = _fixture.Build<Model.Entities.User>()
            .With(x => x.Id, "u_111")
            .With(x => x.IsActive, true)
            .Create();

        _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(setupUser);

        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.GetUserAsync(request.Object, "u_111") as OkObjectResult;
        var result = (StandardResponse<UserDto>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Id.Should().Be("u_111");
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task GetUser_Not_Included_Deleted_User_Returns_Not_Found()
    {
        //Arrange
        var setupUser = _fixture.Build<Model.Entities.User>()
            .With(x => x.Id, "u_111")
            .With(x => x.IsActive, false)
            .Create();

        _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(setupUser);

        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.GetUserAsync(request.Object, "u_111") as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x == "User with userId: u_111 not found");
    }

    [Fact]
    public async Task GetUser_Included_Deleted_User_Returns_User()
    {
        //Arrange
        var setupUser = _fixture.Build<Model.Entities.User>()
            .With(x => x.Id, "u_111")
            .With(x => x.IsActive, false)
            .Create();

        _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(setupUser);

        var queryCollectionItems = new Dictionary<string, StringValues> { { "IncludeDeletedUsers", "True" } };
        var request = MockHttpRequest.Create(query: new QueryCollection(queryCollectionItems));

        //Act
        var response = await _service.GetUserAsync(request.Object, "u_111") as OkObjectResult;
        var result = (StandardResponse<UserDto>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Id.Should().Be("u_111");
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task SetUserActive_Returns_NotFound()
    {
        //Arrange
        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.SetUserActiveAsync(request.Object, "u_111") as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x == "User with userId: u_111 not found");
    }

    [Fact]
    public async Task SetUserActive_Returns_User()
    {
        //Arrange
        var setupUser = _fixture.Build<Model.Entities.User>()
            .With(x => x.Id, "u_111")
            .With(x => x.IsActive, false)
            .Create();

        _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(setupUser);

        _userRepositoryMock.Setup(x => x.SaveUserAsync(It.IsAny<Model.Entities.User>()))
            .ReturnsAsync(setupUser);

        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.SetUserActiveAsync(request.Object, "u_111") as OkObjectResult;
        var result = (StandardResponse<UserDto>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Id.Should().Be("u_111");
        result.Payload.IsActive.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();

        _messagingServiceMock.Verify(x => x.EmitEvent(It.Is<IEvent>(x => x.Type == EventType.UserActive)), Times.Once);
    }

    [Fact]
    public async Task SetUserInActive_Returns_NotFound()
    {
        //Arrange
        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.SetUserInActiveAsync(request.Object, "u_111") as NotFoundObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x == "User with userId: u_111 not found");
    }

    [Fact]
    public async Task SetUserInActive_Returns_User()
    {
        //Arrange
        var setupUser = _fixture.Build<Model.Entities.User>()
            .With(x => x.Id, "u_111")
            .With(x => x.IsActive, true)
            .Create();

        _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(setupUser);

        _userRepositoryMock.Setup(x => x.SaveUserAsync(It.IsAny<Model.Entities.User>()))
            .ReturnsAsync(setupUser);


        var request = MockHttpRequest.Create();

        //Act
        var response = await _service.SetUserInActiveAsync(request.Object, "u_111") as OkObjectResult;
        var result = (StandardResponse<UserDto>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Payload.Should().NotBeNull();
        result.Payload!.Id.Should().Be("u_111");
        result.Payload.IsActive.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();

        _messagingServiceMock.Verify(x => x.EmitEvent(It.Is<IEvent>(x => x.Type == EventType.UserInactive)),
            Times.Once);
    }

    [Fact]
    public async Task CreateUser_Valid_Input_Successfully_Creates_User()
    {
        //Arrange
        var inputUser = new UserDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "mail@example.com"
        };

        var request = MockHttpRequest.Create("/example.com", body: inputUser);

        var setupUser = _fixture.Build<Model.Entities.User>()
            .With(x => x.IsActive, true)
            .With(x => x.IsAdmin, false)
            .With(x => x.FirstName, inputUser.FirstName)
            .With(x => x.LastName, inputUser.LastName)
            .With(x => x.Email, inputUser.Email)
            .Create();
        _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<Model.Entities.User>()))
            .ReturnsAsync(setupUser);

        //Act
        var response = await _service.CreateUserAsync(request.Object) as CreatedResult;
        var result = (StandardResponse<UserDto>)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        result.Payload.Should().NotBeNull();
        result.Payload!.FirstName.Should().Be("Test");
        result.Payload!.LastName.Should().Be("User");
        result.Payload!.Email.Should().Be("mail@example.com");
        result.Payload!.IsActive.Should().BeTrue();
        result.Payload!.IsAdmin.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();

        _messagingServiceMock.Verify(x => x.EmitEvent(It.Is<IEvent>(x => x.Type == EventType.UserCreated)), Times.Once);
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
        var inputUser = new UserDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        var request = MockHttpRequest.Create("/example.com", body: inputUser);


        //Act
        var response = await _service.CreateUserAsync(request.Object) as BadRequestObjectResult;
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
        var inputUser = new UserDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "mail@example.com"
        };


        var request = MockHttpRequest.Create("/example.com", body: inputUser);

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new Model.Entities.User());


        //Act
        var response = await _service.CreateUserAsync(request.Object) as BadRequestObjectResult;
        var result = (StandardResponse)response!.Value;

        //Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}