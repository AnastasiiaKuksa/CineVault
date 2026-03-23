using CineVault.API.Data.Entities;

namespace CineVault.API.Controllers.Requests;

public sealed class ReviewRequest
{
    public required int MovieId { get; init; }
    public required int UserId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }

    public Review ToEntity()
    {
        return new Review
        {
            MovieId = MovieId,
            UserId = UserId,
            Rating = Rating,
            Comment = Comment,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ApplyTo(Review review)
    {
        review.MovieId = MovieId;
        review.UserId = UserId;
        review.Rating = Rating;
        review.Comment = Comment;
    }
}