namespace CineVault.API.Controllers.Responses;

public class OmdbMovieSearchResponse
{
    public string Title { get; set; } = null!;
    public string ReleaseYear { get; set; } = null!;   // береться з Year
    public string? Rated { get; set; }
    public string? Released { get; set; }
    public string? Runtime { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public string? Writer { get; set; }
    public string? Actors { get; set; }
    public string? Description { get; set; }           // береться з Plot
    public string? Awards { get; set; }
    public string? ImdbRating { get; set; }
    public string? RottenTomatoesRating { get; set; }
}