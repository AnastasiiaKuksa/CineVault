using Asp.Versioning;
using CineVault.API.Common.Requests;
using CineVault.API.Common.Responses;
using CineVault.API.Controllers.MoviesV3;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Entities;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/Comments/[action]")]
public sealed class CommentsV3Controller : BaseV3Controller
{
    private readonly ILogger<CommentsV3Controller> logger;
    private readonly CineVaultDbContext dbContext;

    public CommentsV3Controller(
        ILogger<CommentsV3Controller> logger,
        CineVaultDbContext dbContext)
    {
        this.logger = logger;
        this.dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<List<CommentResponse>>>> GetComments(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetComments requested. RequestId: {RequestId}", request.RequestId);
        var comments = await this.dbContext.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .ToListAsync();
        var responses = comments.Select(c => MapComment(c)).ToList();
        return Ok(responses, request.RequestId, "Comments retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<CommentResponse>>> GetCommentById(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Comment {CommentId} requested. RequestId: {RequestId}", id, request.RequestId);
        var comment = await this.dbContext.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null)
        {
            return base.NotFound(new ApiResponse<CommentResponse>
            {
                Success = false,
                Message = $"Comment {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        return Ok(MapComment(comment), request.RequestId, "Comment retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CommentResponse>>> CreateComment(
        [FromBody] ApiRequest<CreateCommentRequest> request)
    {
        this.logger.LogInformation("Creating comment for review {ReviewId}. RequestId: {RequestId}",
            request.Data!.ReviewId, request.RequestId);

        if (request.Data.Rating < 1 || request.Data.Rating > 10)
        {
            return base.BadRequest(new ApiResponse<CommentResponse>
            {
                Success = false,
                Message = "Rating must be between 1 and 10",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var comment = new Comment
        {
            Text = request.Data.Text,
            Rating = request.Data.Rating,
            UserId = request.Data.UserId,
            ReviewId = request.Data.ReviewId
        };

        await this.dbContext.Comments.AddAsync(comment);
        await this.dbContext.SaveChangesAsync();

        var saved = await this.dbContext.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        this.logger.LogInformation("Comment {CommentId} created. RequestId: {RequestId}", comment.Id, request.RequestId);
        return Created(MapComment(saved!), request.RequestId, "Comment created successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<CommentResponse>>> UpdateComment(
        int id, [FromBody] ApiRequest<UpdateCommentRequest> request)
    {
        this.logger.LogInformation("Updating comment {CommentId}. RequestId: {RequestId}", id, request.RequestId);
        var comment = await this.dbContext.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null)
        {
            return base.NotFound(new ApiResponse<CommentResponse>
            {
                Success = false,
                Message = $"Comment {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        if (request.Data!.Rating < 1 || request.Data.Rating > 10)
        {
            return base.BadRequest(new ApiResponse<CommentResponse>
            {
                Success = false,
                Message = "Rating must be between 1 and 10",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        comment.Text = request.Data!.Text;
        comment.Rating = request.Data.Rating;
        this.dbContext.Comments.Update(comment);
        await this.dbContext.SaveChangesAsync();
        return Ok(MapComment(comment), request.RequestId, "Comment updated successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteComment(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Deleting comment {CommentId}. RequestId: {RequestId}", id, request.RequestId);
        var comment = await this.dbContext.Comments.FindAsync(id);
        if (comment is null)
        {
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Comment {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        this.dbContext.Comments.Remove(comment);
        await this.dbContext.SaveChangesAsync();
        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Comment deleted successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }

    [HttpPost("{commentId}/LikeComment")]
    public async Task<ActionResult<ApiResponse<object>>> LikeComment(
        int commentId, [FromBody] ApiRequest<int> request)
    {
        var userId = request.Data;
        this.logger.LogInformation("User {UserId} liking comment {CommentId}. RequestId: {RequestId}",
            userId, commentId, request.RequestId);

        var userExists = await this.dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return base.BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Only registered users can like comments",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var commentExists = await this.dbContext.Comments.AnyAsync(c => c.Id == commentId);
        if (!commentExists)
        {
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Comment {commentId} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var existingLike = await this.dbContext.CommentLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);

        if (existingLike is not null)
        {
            this.dbContext.CommentLikes.Remove(existingLike);
            await this.dbContext.SaveChangesAsync();
            return base.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Like removed",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var like = new CommentLike { UserId = userId, CommentId = commentId };
        await this.dbContext.CommentLikes.AddAsync(like);
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("User {UserId} liked comment {CommentId}. RequestId: {RequestId}",
            userId, commentId, request.RequestId);

        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Comment liked successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }

    private static CommentResponse MapComment(Comment c) => new()
    {
        Id = c.Id,
        Text = c.Text,
        Rating = c.Rating,
        UserId = c.UserId,
        Username = c.User?.Username ?? "",
        ReviewId = c.ReviewId,
        CreatedAt = c.CreatedAt,
        LikesCount = c.Likes.Count
    };
}