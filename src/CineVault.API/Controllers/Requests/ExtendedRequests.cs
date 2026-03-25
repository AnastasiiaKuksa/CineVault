namespace CineVault.API.Controllers.Requests;

public sealed class MovieSearchRequest
{
    public string? Genre { get; set; }
    public string? Title { get; set; }
    public string? Director { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public double? MinRating { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class UserSearchRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public string SortBy { get; set; } = "username";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class CreateCommentRequest
{
    public required int ReviewId { get; init; }
    public required int UserId { get; init; }
    public string? Text { get; init; }
    public required int Rating { get; init; } // 1-10
}

public sealed class UpdateCommentRequest
{
    public string? Text { get; init; }
    public required int Rating { get; init; }
}