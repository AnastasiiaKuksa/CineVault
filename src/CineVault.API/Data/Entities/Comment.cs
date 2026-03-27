namespace CineVault.API.Data.Entities;

public sealed class Comment
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public required int Rating { get; set; }
    public required int UserId { get; set; }
    public required int ReviewId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public User? User { get; set; }
    public Review? Review { get; set; }
    public ICollection<CommentLike> Likes { get; set; } = [];
}