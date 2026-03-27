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
        var users = await this.dbContext.Users
            .AsNoTracking()
            .ToListAsync();
        var response = users.Adapt<IEnumerable<UserResponse>>();
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
            return base.NotFound(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = $"User with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        return Ok(user.Adapt<UserResponse>(), request.RequestId, "User retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<UserStatsResponse>>> GetUserStatus(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetUserStats {UserId}. RequestId: {RequestId}", id, request.RequestId);

        
        var stats = await this.dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserStatsResponse
            {
                UserId = u.Id,
                Username = u.Username,
                TotalReviews = u.Reviews.Count(),
                AverageRating = u.Reviews.Any()
                    ? Math.Round(u.Reviews.Average(r => (double)r.Rating), 2)
                    : 0,
                LastActivity = u.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => (DateTime?)r.CreatedAt)
                    .FirstOrDefault(),
                GenreStats = u.Reviews
                    .Where(r => r.Movie != null && r.Movie.Genre != null)
                    .GroupBy(r => r.Movie!.Genre!)
                    .Select(g => new GenreStatDto
                    {
                        Genre = g.Key,
                        ReviewCount = g.Count(),
                        AverageRating = Math.Round(g.Average(r => (double)r.Rating), 2)
                    })
                    .OrderByDescending(g => g.ReviewCount)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (stats is null)
        {
            return base.NotFound(new ApiResponse<UserStatsResponse>
            {
                Success = false,
                Message = $"User with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        return Ok(stats, request.RequestId, "User stats retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(
        [FromBody] ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation("Creating user {Username}. RequestId: {RequestId}", request.Data!.Username, request.RequestId);

        var emailExists = await this.dbContext.Users
            .AnyAsync(u => u.Email == request.Data!.Email);
        if (emailExists)
        {
            return base.BadRequest(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = $"User with email '{request.Data!.Email}' already exists",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var user = request.Data!.Adapt<User>();
        await this.userRepository.Create(user);
        this.logger.LogInformation("User {Username} created with Id {UserId}. RequestId: {RequestId}", user.Username, user.Id, request.RequestId);
        return Created(user.Adapt<UserResponse>(), request.RequestId, "User created successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PagedResult<UserSearchResponse>>>> SearchUsers(
        [FromBody] ApiRequest<UserSearchRequest> request)
    {
        var filter = request.Data!;
        this.logger.LogInformation("SearchUsers requested. RequestId: {RequestId}", request.RequestId);

        var query = this.dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(filter.Username))
            query = query.Where(u => u.Username.Contains(filter.Username));
        if (!string.IsNullOrEmpty(filter.Email))
            query = query.Where(u => u.Email.Contains(filter.Email));
        if (filter.CreatedAfter.HasValue)
            query = query.Where(u => u.CreatedAt > filter.CreatedAfter.Value);

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
            return base.NotFound(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = $"User with id {id} not found",
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
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"User with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        // Soft delete
        user.IsDeleted = true;
        await this.userRepository.Update(user);
        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User deleted successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }
}