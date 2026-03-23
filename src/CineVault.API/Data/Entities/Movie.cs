namespace CineVault.API.Data.Entities;

public sealed class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public ICollection<Review> Reviews { get; set; } = [];
}