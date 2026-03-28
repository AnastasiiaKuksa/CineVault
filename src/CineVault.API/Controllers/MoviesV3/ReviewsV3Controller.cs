using System.Text.Json;
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
using Microsoft.Extensions.Caching.Distributed;

namespace CineVault.API.Controllers;

[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/Reviews/[action]")]
public sealed class ReviewsV3Controller : BaseV3Controller
{
    private readonly IReviewRepository reviewRepository;
    private readonly ILogger<ReviewsV3Controller> logger;
    private readonly CineVaultDbContext dbContext;
    private readonly IDistributedCache distributedCache;

    public ReviewsV3Controller(
        IReviewRepository reviewRepository,
        ILogger<ReviewsV3Controller> logger,
        CineVaultDbContext dbContext,
        IDistributedCache distributedCache) : base()
    {
        this.reviewRepository = reviewRepository;
        this.logger = logger;
        this.dbContext = dbContext;
        this.distributedCache = distributedCache;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponse>>>> GetReviews(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetReviews requested. RequestId: {RequestId}", request.RequestId);
        var reviews = await this.reviewRepository.GetAllWithDetails();
        var responses = reviews.Adapt<IEnumerable<ReviewResponse>>();
        this.logger.LogInformation("Retrieved {ReviewCount} reviews. RequestId: {RequestId}", responses.Count(), request.RequestId);
        return Ok(responses, request.RequestId, "Reviews retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewById(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Review {ReviewId} requested. RequestId: {RequestId}", id, request.RequestId);
        var review = await this.reviewRepository.GetByIdWithDetails(id);
        if (review is null)
        {
            this.logger.LogWarning("Review {ReviewId} not found. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<ReviewResponse>
            {
                Success = false,
                Message = $"Review {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        return Ok(review.Adapt<ReviewResponse>(), request.RequestId, "Review retrieved successfully");
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview(
      [FromBody] ApiRequest<ReviewRequest> request)
    {
        logger.LogInformation("Creating/updating review (v3) for movie {MovieId} by user {UserId}. RequestId: {RequestId}",
            request.Data?.MovieId, request.Data?.UserId, request.RequestId);

        var existing = await dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r =>
                r.UserId == request.Data!.UserId &&
                r.MovieId == request.Data.MovieId);

        if (existing is not null)
        {
            existing.Rating = request.Data!.Rating;
            existing.Comment = request.Data.Comment;
            await dbContext.SaveChangesAsync();

            //Інвалідація кешу після оновлення огляду
            await distributedCache.RemoveAsync($"reviews_{request.Data.MovieId}");
            logger.LogInformation("Cache invalidated for movie {MovieId} after review update", request.Data.MovieId);

            return Ok(existing.Adapt<ReviewResponse>(), request.RequestId,
                "Review updated (unique per user/movie)");
        }

        var review = request.Data!.Adapt<Review>();
        await dbContext.Reviews.AddAsync(review);
        await dbContext.SaveChangesAsync();

        var created = await dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstAsync(r => r.Id == review.Id);

        //Інвалідація кешу після створення нового огляду
        await distributedCache.RemoveAsync($"reviews_{request.Data.MovieId}");
        logger.LogInformation("Cache invalidated for movie {MovieId} after review creation", request.Data.MovieId);

        return Created(created.Adapt<ReviewResponse>(), request.RequestId, "Review created successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReview(
        int id, [FromBody] ApiRequest<ReviewRequest> request)
    {
        this.logger.LogInformation("Updating review {ReviewId}. RequestId: {RequestId}", id, request.RequestId);
        var review = await this.reviewRepository.GetByIdWithDetails(id);
        if (review is null)
        {
            this.logger.LogWarning("Review {ReviewId} not found for update. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<ReviewResponse>
            {
                Success = false,
                Message = $"Review {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        request.Data!.Adapt(review);
        await this.reviewRepository.Update(review);
        return Ok(review.Adapt<ReviewResponse>(), request.RequestId, "Review updated successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReview(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Deleting review {ReviewId}. RequestId: {RequestId}", id, request.RequestId);
        var review = await this.reviewRepository.GetByIdWithDetails(id);
        if (review is null)
        {
            this.logger.LogWarning("Review {ReviewId} not found for deletion. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Review {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        await this.reviewRepository.Delete(review);
        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Review deleted successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }

    [HttpPost("movies/{movieId:int}/reviews")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponse>>>> GetReviewsByMovie(
       int movieId, [FromBody] ApiRequest<object?> request)
    {
        logger.LogInformation("GetReviewsByMovie {MovieId} requested. RequestId: {RequestId}", movieId, request.RequestId);

        var cacheKey = $"reviews_{movieId}";

        var cachedBytes = await distributedCache.GetAsync(cacheKey);
        if (cachedBytes is not null)
        {
            var cachedReviews = JsonSerializer.Deserialize<IEnumerable<ReviewResponse>>(cachedBytes)
                    ?? Enumerable.Empty<ReviewResponse>();
            logger.LogInformation("Reviews from distributed cache. MovieId: {MovieId}, RequestId: {RequestId}", movieId, request.RequestId);
            return Ok(cachedReviews, request.RequestId, "Reviews from distributed cache");
        }

        logger.LogInformation("Reviews from DB. MovieId: {MovieId}, RequestId: {RequestId}", movieId, request.RequestId);

        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Movie)
            .Include(r => r.User)
            .Where(r => r.MovieId == movieId)
            .ToListAsync();
        var response = reviews.Adapt<IEnumerable<ReviewResponse>>();

        var serialized = JsonSerializer.SerializeToUtf8Bytes(response);
        await distributedCache.SetAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });

        logger.LogInformation("Reviews for movie {MovieId} cached. RequestId: {RequestId}", movieId, request.RequestId);
        return Ok(response, request.RequestId, "Reviews from DB");
    }


}