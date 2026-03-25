using Asp.Versioning;
using CineVault.API.Common.Requests;
using CineVault.API.Common.Responses;
using CineVault.API.Controllers.MoviesV3;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/Users/[action]")]
public sealed class UsersV3Controller : BaseV3Controller
{
    private readonly IUserRepository userRepository;
    private readonly ILogger<UsersV3Controller> logger;

    public UsersV3Controller(IUserRepository userRepository, ILogger<UsersV3Controller> logger)
    {
        this.userRepository = userRepository;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponse>>>> GetUsers(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetUsers requested. RequestId: {RequestId}", request.RequestId);
        var users = await this.userRepository.GetAll();
        var response = users.Select(UserResponse.FromEntity);
        this.logger.LogInformation("Retrieved {UserCount} users. RequestId: {RequestId}", response.Count(), request.RequestId);
        return Ok(response, request.RequestId, "Users retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("User {UserId} requested. RequestId: {RequestId}", id, request.RequestId);
        var user = await this.userRepository.GetById(id);
        if (user is null)
        {
            this.logger.LogWarning("User {UserId} not found. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = $"User {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        return Ok(UserResponse.FromEntity(user), request.RequestId, "User retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(
        [FromBody] ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation("Creating user {Username}. RequestId: {RequestId}", request.Data!.Username, request.RequestId);
        var user = request.Data!.ToEntity();
        await this.userRepository.Create(user);
        this.logger.LogInformation("User {Username} created. RequestId: {RequestId}", request.Data.Username, request.RequestId);
        return Created(UserResponse.FromEntity(user), request.RequestId, "User created successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(
        int id, [FromBody] ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation("Updating user {UserId}. RequestId: {RequestId}", id, request.RequestId);
        var user = await this.userRepository.GetById(id);
        if (user is null)
        {
            this.logger.LogWarning("User {UserId} not found for update. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = $"User {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        request.Data!.ApplyTo(user);
        await this.userRepository.Update(user);
        return Ok(UserResponse.FromEntity(user), request.RequestId, "User updated successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Deleting user {UserId}. RequestId: {RequestId}", id, request.RequestId);
        var user = await this.userRepository.GetById(id);
        if (user is null)
        {
            this.logger.LogWarning("User {UserId} not found for deletion. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"User {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        await this.userRepository.Delete(user);
        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User deleted successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }
}