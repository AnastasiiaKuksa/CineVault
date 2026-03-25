using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers.MoviesV1;

[ApiVersion(1.0, Deprecated = true)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewRepository reviewRepository;

    public ReviewsController(IReviewRepository reviewRepository)
    {
        this.reviewRepository = reviewRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetReviews()
    {
        var reviews = await this.reviewRepository.GetAllWithDetails();
        var responses = reviews.Select(ReviewResponse.FromEntity);
        return Ok(responses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        var review = await this.reviewRepository.GetByIdWithDetails(id);
        if (review is null)
        {
            return NotFound();
        }
        return Ok(ReviewResponse.FromEntity(review));
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