using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;
using EasyGroceries.Common.Utils;
using EasyGroceries.User.Dto;
using EasyGroceries.User.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace EasyGroceries.User.Api;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;
    private readonly IMapper _mapper;
    private readonly IMessagingService _messagingService;
    private readonly IUserRepository _userRepository;

    public HttpTrigger(IMapper mapper, IUserRepository userRepository, IMessagingService messagingService,
        ILogger<HttpTrigger> logger)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _messagingService = messagingService;
        _logger = logger;
    }

    [FunctionName("GetUsersAsync")]
    public async Task<IActionResult> GetUsersAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Users")]
        HttpRequest req)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName} with input: {@Input}",
            nameof(GetUsersAsync), req.Query);

        var includeDeletedUsers = req.Query["IncludeDeletedUsers"];

        return new OkObjectResult(StandardResponse.Success(_mapper.Map<List<UserDto>>(
            await _userRepository.GetUsersAsync((includeDeletedUsers.ToString() ?? string.Empty).ToLowerInvariant() ==
                                                bool.TrueString.ToLowerInvariant()))));
    }

    [FunctionName("GetUserAsync")]
    public async Task<IActionResult> GetUserAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Users/{userId}")]
        HttpRequest req, string userId)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName} with input: {Input}",
            nameof(GetUserAsync), $"userId, {req.Query}");

        var includeDeletedUsers = req.Query["IncludeDeletedUsers"];

        var user = await _userRepository.GetUserAsync(userId);
        if (user == null)
        {
            var errorMessage = $"User with userId: {userId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }

        if ((includeDeletedUsers.ToString() ?? string.Empty).ToLowerInvariant() == bool.TrueString.ToLowerInvariant() ||
            user.IsActive)
        {
            return new OkObjectResult(StandardResponse.Success(_mapper.Map<UserDto>(user)));
        }
        else
        {
            var errorMessage = $"User with userId: {userId} not found";
            _logger.LogError("{Message}", errorMessage);
            return new NotFoundObjectResult(StandardResponse.Failure(errorMessage));
        }
    }

    [FunctionName("CreateUserAsync")]
    public async Task<IActionResult> CreateUserAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Users")]
        HttpRequest req)
    {
        var user = await req.GetBody<UserDto>();

        _logger.LogInformation("Processing request for MethodName: {MethodName} with input: {@Input}",
            nameof(CreateUserAsync), user);

        if (!DtoValidation.IsValid(user, out var result)) return result;

        var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
        if (existingUser != null)
        {
            var errorMessage = "User with same email address already exists. Use a different email address";
            _logger.LogError("{Message}", errorMessage);
            return new BadRequestObjectResult(StandardResponse.Failure(errorMessage));
        }

        var dbResponse = await _userRepository.CreateUserAsync(_mapper.Map<Model.Entities.User>(user));

        await _messagingService.EmitEvent(_mapper.Map<UserCreatedEvent>(dbResponse));

        return new CreatedResult(new Uri(req.Path.ToUriComponent()),
            StandardResponse.Success(_mapper.Map<UserDto>(dbResponse)));
    }

    [FunctionName("SetUserActiveAsync")]
    public async Task<IActionResult> SetUserActiveAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Users/Active/{userId}")]
        HttpRequest req, string userId)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName} with input: {Input}",
            nameof(SetUserActiveAsync), userId);

        var user = await _userRepository.GetUserAsync(userId);
        if (user == null)
        {
            _logger.LogError("{Message}", $"User with userId: {userId} not found");
            return new NotFoundObjectResult(StandardResponse.Failure($"User with userId: {userId} not found"));
        }

        _logger.LogInformation("User: {@User} found", user);
        user.IsActive = true;
        _logger.LogDebug("User: {@User} updated", user);

        var dbResponse = await _userRepository.SaveUserAsync(user);

        await _messagingService.EmitEvent(_mapper.Map<UserActiveEvent>(dbResponse));


        return new OkObjectResult(StandardResponse.Success(_mapper.Map<UserDto>(dbResponse)));
    }

    [FunctionName("SetUserInActiveAsync")]
    public async Task<IActionResult> SetUserInActiveAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Users/InActive/{userId}")]
        HttpRequest req, string userId)
    {
        _logger.LogInformation("Processing request for MethodName: {MethodName} with input: {Input}",
            nameof(SetUserInActiveAsync), userId);

        var user = await _userRepository.GetUserAsync(userId);
        if (user == null)
        {
            _logger.LogError("{Message}", $"User with userId: {userId} not found");
            return new NotFoundObjectResult(StandardResponse.Failure($"User with userId: {userId} not found"));
        }

        _logger.LogInformation("User: {@User} found", user);
        user.IsActive = false;
        _logger.LogDebug("User: {@User} updated", user);

        var dbResponse = await _userRepository.SaveUserAsync(user);

        await _messagingService.EmitEvent(_mapper.Map<UserInactiveEvent>(dbResponse));

        return new OkObjectResult(StandardResponse.Success(_mapper.Map<UserDto>(dbResponse)));
    }
}