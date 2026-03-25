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
[Route("api/v{version:apiVersion}/Reviews/[action]")]
public sealed class ReviewsV3Controller : BaseV3Controller
{
    private readonly IReviewRepository reviewRepository;
    private readonly ILogger<ReviewsV3Controller> logger;
    private readonly CineVaultDbContext dbContext;

    public ReviewsV3Controller(
        IReviewRepository reviewRepository,
        ILogger<ReviewsV3Controller> logger,
        CineVaultDbContext dbContext)
    {
        this.reviewRepository = reviewRepository;
        this.logger = logger;
        this.dbContext = dbContext;
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview(
        [FromBody] ApiRequest<ReviewRequest> request)
    {
        this.logger.LogInformation("Creating review. RequestId: {RequestId}", request.RequestId);

        // Unique review per user per movie - update if exists
        var existing = await this.dbContext.Reviews
            .FirstOrDefaultAsync(r => r.UserId == request.Data!.UserId && r.MovieId == request.Data.MovieId);

        if (existing is not null)
        {
            this.logger.LogInformation("Review already exists for User {UserId} Movie {MovieId} - updating. RequestId: {RequestId}",
                request.Data!.UserId, request.Data.MovieId, request.RequestId);
            existing.Rating = request.Data!.Rating;
            existing.Comment = request.Data.Comment;
            await this.reviewRepository.Update(existing);
            var updatedReview = await this.reviewRepository.GetByIdWithDetails(existing.Id);
            return Ok(updatedReview!.Adapt<ReviewResponse>(), request.RequestId, "Review updated (already existed)");
        }

        var review = request.Data!.Adapt<Review>();
        await this.reviewRepository.Create(review);
        var reviewWithDetails = await this.reviewRepository.GetByIdWithDetails(review.Id);
        this.logger.LogInformation("Review created with Id {ReviewId}. RequestId: {RequestId}", review.Id, request.RequestId);
        return Created(reviewWithDetails!.Adapt<ReviewResponse>(), request.RequestId, "Review created successfully");
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
}