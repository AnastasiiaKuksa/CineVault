namespace CineVault.API.Data.Entities;

public sealed class CommentLike
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required int CommentId { get; set; }
    public User? User { get; set; }
    public Comment? Comment { get; set; }
}