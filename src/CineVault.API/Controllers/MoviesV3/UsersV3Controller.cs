using Asp.Versioning;
using CineVault.API.Common.Requests;
using CineVault.API.Common.Responses;
using CineVault.API.Controllers.MoviesV3;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/Users/[action]")]
public sealed class UsersV3Controller : BaseV3Controller
{
    private readonly IUserRepository userRepository;
    private readonly ILogger<UsersV3Controller> logger;
    private readonly CineVaultDbContext dbContext;

    public UsersV3Controller(
        IUserRepository userRepository,
        ILogger<UsersV3Controller> logger,
        CineVaultDbContext dbContext)
    {
        this.userRepository = userRepository;
        this.logger = logger;
        this.dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponse>>>> GetUsers(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetUsers requested. RequestId: {RequestId}", request.RequestId);
        var users = await this.userRepository.GetAll();
        var response = users.Adapt<IEnumerable<UserResponse>>();
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
        return Ok(user.Adapt<UserResponse>(), request.RequestId, "User retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(
        [FromBody] ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation("Creating user {Username}. RequestId: {RequestId}", request.Data!.Username, request.RequestId);
        var user = request.Data!.Adapt<User>();
        await this.userRepository.Create(user);
        var response = user.Adapt<UserResponse>();
        this.logger.LogInformation("User {Username} created with Id {UserId}. RequestId: {RequestId}", user.Username, user.Id, request.RequestId);
        return Created(response, request.RequestId, "User created successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PagedResult<UserSearchResponse>>>> SearchUsers(
        [FromBody] ApiRequest<UserSearchRequest> request)
    {
        var filter = request.Data!;
        this.logger.LogInformation("SearchUsers requested. RequestId: {RequestId}", request.RequestId);

        var query = this.dbContext.Users.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Username))
            query = query.Where(u => u.Username.Contains(filter.Username));
        if (!string.IsNullOrEmpty(filter.Email))
            query = query.Where(u => u.Email.Contains(filter.Email));

        query = filter.SortBy switch
        {
            "email" => query.OrderBy(u => u.Email),
            _ => query.OrderBy(u => u.Username)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<UserSearchResponse>
        {
            Items = items.Adapt<List<UserSearchResponse>>(),
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        this.logger.LogInformation("SearchUsers found {Total} users. RequestId: {RequestId}", total, request.RequestId);
        return Ok(result, request.RequestId, "Users found");
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
        request.Data!.Adapt(user);
        await this.userRepository.Update(user);
        return Ok(user.Adapt<UserResponse>(), request.RequestId, "User updated successfully");
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