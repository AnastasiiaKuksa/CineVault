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
[Route("api/v{version:apiVersion}/Actors/[action]")]
public sealed class ActorsV3Controller : BaseV3Controller
{
    private readonly ILogger<ActorsV3Controller> logger;
    private readonly CineVaultDbContext dbContext;

    public ActorsV3Controller(
        ILogger<ActorsV3Controller> logger,
        CineVaultDbContext dbContext)
    {
        this.logger = logger;
        this.dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<List<ActorResponse>>>> GetActors(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetActors requested. RequestId: {RequestId}", request.RequestId);
        var actors = await this.dbContext.Actors
            .AsNoTracking()
            .Include(a => a.MovieActors)
            .ThenInclude(ma => ma.Movie)
            .ToListAsync();
        var response = actors.Adapt<List<ActorResponse>>();
        return Ok(response, request.RequestId, "Actors retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> GetActorById(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Actor {ActorId} requested. RequestId: {RequestId}", id, request.RequestId);
        var actor = await this.dbContext.Actors
            .AsNoTracking()
            .Include(a => a.MovieActors)
            .ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (actor is null)
        {
            return base.NotFound(new ApiResponse<ActorResponse>
            {
                Success = false,
                Message = $"Actor with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        return Ok(actor.Adapt<ActorResponse>(), request.RequestId, "Actor retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ActorCreatedResponse>>> CreateActor(
        [FromBody] ApiRequest<CreateActorRequest> request)
    {
        this.logger.LogInformation("Creating actor {FullName}. RequestId: {RequestId}", request.Data!.FullName, request.RequestId);
        var actor = request.Data!.Adapt<Actor>();
        await this.dbContext.Actors.AddAsync(actor);
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("Actor {FullName} created with Id {ActorId}. RequestId: {RequestId}", actor.FullName, actor.Id, request.RequestId);
        return Created(new ActorCreatedResponse { Id = actor.Id, FullName = actor.FullName }, request.RequestId, "Actor created successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<List<ActorCreatedResponse>>>> BulkCreateActors(
        [FromBody] ApiRequest<List<CreateActorRequest>> request)
    {
        this.logger.LogInformation("BulkCreate {Count} actors. RequestId: {RequestId}", request.Data!.Count, request.RequestId);
        var actors = request.Data!.Adapt<List<Actor>>();
        await this.dbContext.Actors.AddRangeAsync(actors);
        await this.dbContext.SaveChangesAsync();
        var response = actors.Select(a => new ActorCreatedResponse { Id = a.Id, FullName = a.FullName }).ToList();
        this.logger.LogInformation("Bulk created {Count} actors. RequestId: {RequestId}", actors.Count, request.RequestId);
        return Created(response, request.RequestId, $"{actors.Count} actors created successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> UpdateActor(
        int id, [FromBody] ApiRequest<CreateActorRequest> request)
    {
        this.logger.LogInformation("Updating actor {ActorId}. RequestId: {RequestId}", id, request.RequestId);
        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            return base.NotFound(new ApiResponse<ActorResponse>
            {
                Success = false,
                Message = $"Actor with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        request.Data!.Adapt(actor);
        this.dbContext.Actors.Update(actor);
        await this.dbContext.SaveChangesAsync();
        return Ok(actor.Adapt<ActorResponse>(), request.RequestId, "Actor updated successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteActor(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Deleting actor {ActorId}. RequestId: {RequestId}", id, request.RequestId);
        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Actor with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        // Soft delete
        actor.IsDeleted = true;
        this.dbContext.Actors.Update(actor);
        await this.dbContext.SaveChangesAsync();
        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Actor deleted successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }

    [HttpPost("{movieId}/AddActor/{actorId}")]
    public async Task<ActionResult<ApiResponse<object>>> AddActorToMovie(
        int movieId, int actorId, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Adding actor {ActorId} to movie {MovieId}. RequestId: {RequestId}", actorId, movieId, request.RequestId);

        var movieExists = await this.dbContext.Movies.AnyAsync(m => m.Id == movieId);
        if (!movieExists)
        {
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Movie with id {movieId} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var actorExists = await this.dbContext.Actors.AnyAsync(a => a.Id == actorId);
        if (!actorExists)
        {
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Actor with id {actorId} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var alreadyLinked = await this.dbContext.MovieActors
            .AnyAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

        if (alreadyLinked)
        {
            return base.BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Actor {actorId} is already linked to movie {movieId}",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        await this.dbContext.MovieActors.AddAsync(new MovieActor { MovieId = movieId, ActorId = actorId });
        await this.dbContext.SaveChangesAsync();

        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Actor added to movie successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }
}