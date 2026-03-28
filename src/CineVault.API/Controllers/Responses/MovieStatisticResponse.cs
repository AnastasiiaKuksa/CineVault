namespace CineVault.API.Common.Responses;

public class MovieStatisticResponse
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; }
}