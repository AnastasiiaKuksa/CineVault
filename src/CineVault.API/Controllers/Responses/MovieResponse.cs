using CineVault.API.Data.Entities;

namespace CineVault.API.Controllers.Responses;

public sealed class MovieResponse
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public required double AverageRating { get; set; }
    public required int ReviewCount { get; set; }
    /// нові поля
    public string? Runtime { get; set; }
    public string? Awards { get; set; }
    public string? ImdbRating { get; set; }
    public string? RottenTomatoesRating { get; set; }

    public static MovieResponse FromEntity(Movie movie)
    {
        return new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = movie.Reviews.Count != 0
                ? movie.Reviews.Average(r => r.Rating)
                : 0,
            ReviewCount = movie.Reviews.Count
        };
    }
}