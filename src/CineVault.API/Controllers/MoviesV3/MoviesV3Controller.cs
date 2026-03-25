using Asp.Versioning;
using CineVault.API.Common.Requests;
using CineVault.API.Common.Responses;
using CineVault.API.Controllers.MoviesV3;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/Movies/[action]")]
public sealed class MoviesV3Controller : BaseV3Controller
{
    private readonly IMovieRepository movieRepository;
    private readonly ILogger<MoviesV3Controller> logger;
    private readonly IMapper mapper;
    private readonly CineVault.API.Data.Entities.CineVaultDbContext dbContext;

    public MoviesV3Controller(
        IMovieRepository movieRepository,
        ILogger<MoviesV3Controller> logger,
        IMapper mapper,
        CineVault.API.Data.Entities.CineVaultDbContext dbContext)
    {
        this.movieRepository = movieRepository;
        this.logger = logger;
        this.mapper = mapper;
        this.dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovieResponse>>>> GetMovies(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetMovies requested. RequestId: {RequestId}", request.RequestId);
        var movies = await this.movieRepository.GetAll();
        var response = this.mapper.Map<IEnumerable<MovieResponse>>(movies);
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
        var response = this.mapper.Map<MovieResponse>(movie);
        return Ok(response, request.RequestId, "Movie retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MovieCreatedResponse>>> CreateMovie(
        [FromBody] ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("Creating movie {MovieTitle}. RequestId: {RequestId}", request.Data!.Title, request.RequestId);
        var movie = this.mapper.Map<Movie>(request.Data!);
        await this.movieRepository.Create(movie);
        var response = new MovieCreatedResponse { Id = movie.Id, Title = movie.Title };
        this.logger.LogInformation("Movie {MovieTitle} created with Id {MovieId}. RequestId: {RequestId}", movie.Title, movie.Id, request.RequestId);
        return Created(response, request.RequestId, "Movie created successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<List<MovieCreatedResponse>>>> BulkCreateMovies(
        [FromBody] ApiRequest<List<MovieRequest>> request)
    {
        this.logger.LogInformation("BulkCreate {Count} movies. RequestId: {RequestId}", request.Data!.Count, request.RequestId);
        var movies = request.Data!.Adapt<List<Movie>>();
        await this.dbContext.Movies.AddRangeAsync(movies);
        await this.dbContext.SaveChangesAsync();
        var response = movies.Select(m => new MovieCreatedResponse { Id = m.Id, Title = m.Title }).ToList();
        this.logger.LogInformation("Bulk created {Count} movies. RequestId: {RequestId}", movies.Count, request.RequestId);
        return Created(response, request.RequestId, $"{movies.Count} movies created successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PagedResult<MovieResponse>>>> SearchMovies(
        [FromBody] ApiRequest<MovieSearchRequest> request)
    {
        var filter = request.Data!;
        this.logger.LogInformation("SearchMovies requested. RequestId: {RequestId}", request.RequestId);

        var query = this.dbContext.Movies.Include(m => m.Reviews).AsQueryable();

        if (!string.IsNullOrEmpty(filter.Genre))
            query = query.Where(m => m.Genre != null && m.Genre.Contains(filter.Genre));
        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(m => m.Title.Contains(filter.Title));
        if (!string.IsNullOrEmpty(filter.Director))
            query = query.Where(m => m.Director != null && m.Director.Contains(filter.Director));
        if (filter.YearFrom.HasValue)
            query = query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year >= filter.YearFrom.Value);
        if (filter.YearTo.HasValue)
            query = query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year <= filter.YearTo.Value);
        if (filter.MinRating.HasValue)
            query = query.Where(m => m.Reviews.Any() && m.Reviews.Average(r => r.Rating) >= filter.MinRating.Value);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<MovieResponse>
        {
            Items = this.mapper.Map<List<MovieResponse>>(items),
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        this.logger.LogInformation("SearchMovies found {Total} results. RequestId: {RequestId}", total, request.RequestId);
        return Ok(result, request.RequestId, "Movies found");
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
        this.mapper.Map(request.Data!, movie);
        await this.movieRepository.Update(movie);
        var response = this.mapper.Map<MovieResponse>(movie);
        return Ok(response, request.RequestId, "Movie updated successfully");
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
        return base.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Movie deleted successfully",
            RequestId = request.RequestId,
            ApiVersion = "v3"
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BulkDeleteResult>>> BulkDeleteMovies(
        [FromBody] ApiRequest<List<int>> request)
    {
        var ids = request.Data!;
        this.logger.LogInformation("BulkDelete {Count} movies. RequestId: {RequestId}", ids.Count, request.RequestId);

        var result = new BulkDeleteResult();

        foreach (var id in ids)
        {
            var hasReviews = await this.dbContext.Reviews.AnyAsync(r => r.MovieId == id);
            if (hasReviews)
            {
                var warning = $"Movie {id} has reviews and was not deleted";
                this.logger.LogWarning(warning + " RequestId: {RequestId}", request.RequestId);
                result.Warnings.Add(warning);
                continue;
            }

            var movie = await this.dbContext.Movies.FindAsync(id);
            if (movie is not null)
            {
                this.dbContext.Movies.Remove(movie);
                result.DeletedIds.Add(id);
            }
        }

        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("BulkDelete completed. Deleted: {Deleted}, Skipped: {Skipped}. RequestId: {RequestId}",
            result.DeletedIds.Count, result.Warnings.Count, request.RequestId);

        return Ok(result, request.RequestId, $"Deleted {result.DeletedIds.Count} movies, skipped {result.Warnings.Count}");
    }
}