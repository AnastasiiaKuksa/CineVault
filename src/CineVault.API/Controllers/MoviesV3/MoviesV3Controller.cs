using Asp.Versioning;
using CineVault.API.Common.Requests;
using CineVault.API.Common.Responses;
using CineVault.API.Controllers.MoviesV3;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/Movies/[action]")]
public sealed class MoviesV3Controller : BaseV3Controller
{
    private readonly IMovieRepository movieRepository;
    private readonly ILogger<MoviesV3Controller> logger;

    public MoviesV3Controller(IMovieRepository movieRepository, ILogger<MoviesV3Controller> logger)
    {
        this.movieRepository = movieRepository;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovieResponse>>>> GetMovies(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetMovies requested. RequestId: {RequestId}", request.RequestId);
        var movies = await this.movieRepository.GetAll();
        var response = movies.Select(MovieResponse.FromEntity);
        this.logger.LogInformation("Retrieved {MovieCount} movies. RequestId: {RequestId}", response.Count(), request.RequestId);
        return Ok(response, request.RequestId, "Movies retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieById(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Movie {MovieId} requested. RequestId: {RequestId}", id, request.RequestId);
        var movie = await this.movieRepository.GetById(id);
        if (movie is null)
        {
            this.logger.LogWarning("Movie {MovieId} not found. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<MovieResponse>
            {
                Success = false,
                Message = $"Movie {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        this.logger.LogInformation("Movie {MovieId} retrieved. RequestId: {RequestId}", id, request.RequestId);
        return Ok(MovieResponse.FromEntity(movie), request.RequestId, "Movie retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> CreateMovie(
        [FromBody] ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("Creating movie {MovieTitle}. RequestId: {RequestId}", request.Data!.Title, request.RequestId);
        var movie = request.Data!.ToEntity();
        await this.movieRepository.Create(movie);
        this.logger.LogInformation("Movie {MovieTitle} created. RequestId: {RequestId}", request.Data.Title, request.RequestId);
        return Created(MovieResponse.FromEntity(movie), request.RequestId, "Movie created successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UpdateMovie(
        int id, [FromBody] ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("Updating movie {MovieId}. RequestId: {RequestId}", id, request.RequestId);
        var movie = await this.movieRepository.GetById(id);
        if (movie is null)
        {
            this.logger.LogWarning("Movie {MovieId} not found for update. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<MovieResponse>
            {
                Success = false,
                Message = $"Movie {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        request.Data!.ApplyTo(movie);
        await this.movieRepository.Update(movie);
        this.logger.LogInformation("Movie {MovieId} updated. RequestId: {RequestId}", id, request.RequestId);
        return Ok(MovieResponse.FromEntity(movie), request.RequestId, "Movie updated successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMovie(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Deleting movie {MovieId}. RequestId: {RequestId}", id, request.RequestId);
        var movie = await this.movieRepository.GetById(id);
        if (movie is null)
        {
            this.logger.LogWarning("Movie {MovieId} not found for deletion. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Movie {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        await this.movieRepository.Delete(movie);
        this.logger.LogInformation("Movie {MovieId} deleted. RequestId: {RequestId}", id, request.RequestId);
        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Movie deleted successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }
}