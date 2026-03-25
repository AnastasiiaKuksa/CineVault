using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers.MoviesV2;

[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/Reviews/[action]")]
public sealed class ReviewsV2Controller : ControllerBase
{
    private readonly IReviewRepository reviewRepository;

    public ReviewsV2Controller(IReviewRepository reviewRepository)
    {
        this.reviewRepository = reviewRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetReviews()
    {
        var reviews = await this.reviewRepository.GetAllWithDetails();
        var responses = reviews.Select(ReviewResponse.FromEntity);
        return Ok(new
        {
            Version = "v2",
            Count = responses.Count(),
            Data = responses
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        var review = await this.reviewRepository.GetByIdWithDetails(id);
        if (review is null)
        {
            return NotFound();
        }
        return Ok(new
        {
            Version = "v2",
            Data = ReviewResponse.FromEntity(review)
        });
    }

    [HttpPost]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        var review = request.ToEntity();
        await this.reviewRepository.Create(review);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        var review = await this.reviewRepository.GetByIdWithDetails(id);
        if (review is null)
        {
            return NotFound();
        }
        request.ApplyTo(review);
        await this.reviewRepository.Update(review);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        var review = await this.reviewRepository.GetByIdWithDetails(id);
        if (review is null)
        {
            return NotFound();
        }
        await this.reviewRepository.Delete(review);
        return NoContent();
    }
}