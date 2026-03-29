namespace CineVault.API.Data.Entities;

public sealed class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public bool IsDeleted { get; set; } = false;
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<MovieActor> MovieActors { get; set; } = [];
    public string? PosterUrl { get; set; }
}