using CineVault.API.Data.Entities;

namespace CineVault.API.Controllers.Requests;

public sealed class MovieRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public string? Genre { get; init; }
    public string? Director { get; init; }

    public Movie ToEntity()
    {
        return new Movie
        {
            Title = Title,
            Description = Description,
            ReleaseDate = ReleaseDate,
            Genre = Genre,
            Director = Director
        };
    }

    public void ApplyTo(Movie movie)
    {
        movie.Title = Title;
        movie.Description = Description;
        movie.ReleaseDate = ReleaseDate;
        movie.Genre = Genre;
        movie.Director = Director;
    }
}