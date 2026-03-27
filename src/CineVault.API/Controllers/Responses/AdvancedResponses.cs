namespace CineVault.API.Controllers.Responses;

public sealed class MovieDetailsResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<string> Actors { get; set; } = [];
    public List<RecentReviewDto> RecentReviews { get; set; } = [];
}

public sealed class RecentReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Username { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public sealed class UserStatsResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public DateTime? LastActivity { get; set; }
    public List<GenreStatDto> GenreStats { get; set; } = [];
}

public sealed class GenreStatDto
{
    public string Genre { get; set; } = null!;
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
}