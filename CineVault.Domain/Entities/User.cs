namespace CineVault.API.Data.Entities;

public sealed class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<CommentLike> CommentLikes { get; set; } = [];
    public DateTime? LastLoginAt { get; set; }
}