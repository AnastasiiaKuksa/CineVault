namespace CineVault.API.Data.Entities;

public sealed class Review
{
    public int Id { get; set; }
    public required int MovieId { get; set; }
    public required int UserId { get; set; }
    public required int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public Movie? Movie { get; set; }
    public User? User { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];
    public DateTime? UpdatedAt { get; set; }
}