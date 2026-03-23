using CineVault.API.Data.Entities;

namespace CineVault.API.Controllers.Responses;

public sealed class ReviewResponse
{
    public required int Id { get; set; }
    public required int MovieId { get; set; }
    public required string MovieTitle { get; set; }
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public required int Rating { get; set; }
    public string? Comment { get; set; }
    public required DateTime CreatedAt { get; set; }

    public static ReviewResponse FromEntity(Review review)
    {
        return new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}