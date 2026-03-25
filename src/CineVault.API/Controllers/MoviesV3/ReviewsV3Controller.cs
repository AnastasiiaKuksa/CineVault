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
[Route("api/v{version:apiVersion}/Reviews/[action]")]
public sealed class ReviewsV3Controller : BaseV3Controller
{
    private readonly IReviewRepository reviewRepository;
    private readonly ILogger<ReviewsV3Controller> logger;

    public ReviewsV3Controller(IReviewRepository reviewRepository, ILogger<ReviewsV3Controller> logger)
    {
        this.reviewRepository = reviewRepository;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponse>>>> GetReviews(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetReviews requested. RequestId: {RequestId}", request.RequestId);
        var reviews = await this.reviewRepository.GetAllWithDetails();
        var responses = reviews.Select(ReviewResponse.FromEntity);
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
        return Ok(ReviewResponse.FromEntity(review), request.RequestId, "Review retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview(
        [FromBody] ApiRequest<ReviewRequest> request)
    {
        this.logger.LogInformation("Creating review. RequestId: {RequestId}", request.RequestId);
        var review = request.Data!.ToEntity();
        await this.reviewRepository.Create(review);
        this.logger.LogInformation("Review created. RequestId: {RequestId}", request.RequestId);
        return Created(ReviewResponse.FromEntity(review), request.RequestId, "Review created successfully");
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
        request.Data!.ApplyTo(review);
        await this.reviewRepository.Update(review);
        return Ok(ReviewResponse.FromEntity(review), request.RequestId, "Review updated successfully");
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