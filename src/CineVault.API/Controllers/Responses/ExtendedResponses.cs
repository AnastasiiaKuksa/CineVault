namespace CineVault.API.Controllers.Responses;

public sealed class MovieCreatedResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
}

public sealed class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public sealed class UserSearchResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public sealed class CommentResponse
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public int Rating { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int ReviewId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
}

public sealed class BulkDeleteResult
{
    public List<int> DeletedIds { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}