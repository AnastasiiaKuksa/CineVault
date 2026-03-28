namespace CineVault.API.Data.Entities;

public sealed class MovieStatistic
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public Movie? Movie { get; set; }
}