using Asp.Versioning;
using CineVault.API.Common.Requests;
using CineVault.API.Common.Responses;
using CineVault.API.Controllers.MoviesV3;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using CineVault.API.Requests;
using CineVault.API.Responses.Omdb;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CineVault.API.Controllers;

[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/Movies/[action]")]
public sealed class MoviesV3Controller : BaseV3Controller
{
    private readonly IMovieRepository movieRepository;
    private readonly ILogger<MoviesV3Controller> logger;
    private readonly IMapper mapper;
    private readonly CineVaultDbContext dbContext;
    private readonly IMemoryCache memoryCache;

    private readonly IOmdbService omdbService;

    private const string MovieSearchCacheKey = "movies_search_cache";

    private static readonly Func<CineVaultDbContext, int, Task<Movie?>> GetMovieByIdCompiled =
        EF.CompileAsyncQuery((CineVaultDbContext ctx, int id) =>
            ctx.Movies
                .Include(m => m.Reviews)
                .FirstOrDefault(m => m.Id == id));

    public MoviesV3Controller(
       IMovieRepository movieRepository,
       ILogger<MoviesV3Controller> logger,
       IMapper mapper,
       CineVaultDbContext dbContext,
       IMemoryCache memoryCache,
       IOmdbService omdbService)
    {
        this.movieRepository = movieRepository;
        this.logger = logger;
        this.mapper = mapper;
        this.dbContext = dbContext;
        this.memoryCache = memoryCache;
        this.omdbService = omdbService;
    }


    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<List<MovieStatisticResponse>>>> GetStatistics()
    {
        this.logger.LogInformation("GetStatistics requested (v3)");

        var stats = await this.dbContext.MovieStatistics
            .AsNoTracking()
            .ProjectToType<MovieStatisticResponse>()
            .ToListAsync();

        this.logger.LogInformation("GetStatistics returned {Count} records", stats.Count);
        return Ok(stats, string.Empty, "Movie statistics retrieved");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IEnumerable<MovieResponse>>>> GetMovies(
        [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetMovies requested. RequestId: {RequestId}", request.RequestId);
        var movies = await this.dbContext.Movies
            .AsNoTracking()
            .Include(m => m.Reviews)
            .ToListAsync();
        var response = this.mapper.Map<IEnumerable<MovieResponse>>(movies);
        this.logger.LogInformation("Retrieved {MovieCount} movies. RequestId: {RequestId}", response.Count(), request.RequestId);
        return Ok(response, request.RequestId, "Movies retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieById(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("Movie {MovieId} requested. RequestId: {RequestId}", id, request.RequestId);
        var movie = await GetMovieByIdCompiled(this.dbContext, id);
        if (movie is null)
        {
            this.logger.LogWarning("Movie {MovieId} not found. RequestId: {RequestId}", id, request.RequestId);
            return base.NotFound(new ApiResponse<MovieResponse>
            {
                Success = false,
                Message = $"Movie with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        var response = this.mapper.Map<MovieResponse>(movie);
        return Ok(response, request.RequestId, "Movie retrieved successfully");
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ApiResponse<MovieDetailsResponse>>> GetMovieDetails(
        int id, [FromBody] ApiRequest request)
    {
        this.logger.LogInformation("GetMovieDetails {MovieId}. RequestId: {RequestId}", id, request.RequestId);

        // Single optimized query with Select - loads only needed fields
        var details = await this.dbContext.Movies
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MovieDetailsResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Genre = m.Genre,
                Director = m.Director,
                ReleaseDate = m.ReleaseDate,
                AverageRating = m.Reviews.Any()
                    ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 2)
                    : 0,
                ReviewCount = m.Reviews.Count(),
                Actors = m.MovieActors
                    .Select(ma => ma.Actor!.FullName)
                    .ToList(),
                RecentReviews = m.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new RecentReviewDto
                    {
                        Id = r.Id,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        Username = r.User!.Username,
                        CreatedAt = r.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (details is null)
        {
            return base.NotFound(new ApiResponse<MovieDetailsResponse>
            {
                Success = false,
                Message = $"Movie with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        return Ok(details, request.RequestId, "Movie details retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MovieCreatedResponse>>> CreateMovie(
        [FromBody] ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("Creating movie {MovieTitle}. RequestId: {RequestId}", request.Data!.Title, request.RequestId);

        var titleExists = await this.dbContext.Movies
            .AnyAsync(m => m.Title == request.Data!.Title);
        if (titleExists)
        {
            return base.BadRequest(new ApiResponse<MovieCreatedResponse>
            {
                Success = false,
                Message = $"Movie with title '{request.Data!.Title}' already exists",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }

        var movie = this.mapper.Map<Movie>(request.Data!);
        await this.movieRepository.Create(movie);
        this.logger.LogInformation("Movie {MovieTitle} created with Id {MovieId}. RequestId: {RequestId}", movie.Title, movie.Id, request.RequestId);
        return Created(new MovieCreatedResponse { Id = movie.Id, Title = movie.Title }, request.RequestId, "Movie created successfully");
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

        var query = this.dbContext.Movies
            .AsNoTracking()
            .Include(m => m.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(m => m.Title.Contains(filter.Title)
                || (m.Description != null && m.Description.Contains(filter.Title))
                || (m.Director != null && m.Director.Contains(filter.Title)));
        if (!string.IsNullOrEmpty(filter.Genre))
            query = query.Where(m => m.Genre != null && m.Genre.Contains(filter.Genre));
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
            return base.NotFound(new ApiResponse<MovieResponse>
            {
                Success = false,
                Message = $"Movie with id {id} not found",
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
            return base.NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"Movie with id {id} not found",
                RequestId = request.RequestId,
                ApiVersion = "v3"
            });
        }
        // Soft delete
        movie.IsDeleted = true;
        await this.movieRepository.Update(movie);
        this.logger.LogInformation("Movie {MovieId} soft deleted. RequestId: {RequestId}", id, request.RequestId);
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
                movie.IsDeleted = true;
                result.DeletedIds.Add(id);
            }
        }

        await this.dbContext.SaveChangesAsync();
        return Ok(result, request.RequestId, $"Deleted {result.DeletedIds.Count} movies, skipped {result.Warnings.Count}");
    }

    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<MovieResponse>>>> Search(
       [FromBody] ApiRequest<MovieSearchRequest> request)
    {
        var filter = request.Data!;
        this.logger.LogInformation("SearchMovies requested. RequestId: {RequestId}", request.RequestId);

        if (this.memoryCache.TryGetValue(MovieSearchCacheKey, out PagedResult<MovieResponse>? cachedResult))
        {
            this.logger.LogInformation("Data from cache. RequestId: {RequestId}", request.RequestId);
            return Ok(cachedResult!, request.RequestId, "Movies found (from cache)");
        }

        this.logger.LogInformation("Data from DB. RequestId: {RequestId}", request.RequestId);

        var query = this.dbContext.Movies
            .AsNoTracking()
            .Include(m => m.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(m => m.Title.Contains(filter.Title)
                || (m.Description != null && m.Description.Contains(filter.Title))
                || (m.Director != null && m.Director.Contains(filter.Title)));
        if (!string.IsNullOrEmpty(filter.Genre))
            query = query.Where(m => m.Genre != null && m.Genre.Contains(filter.Genre));
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

        this.memoryCache.Set(MovieSearchCacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
        });

        this.logger.LogInformation("SearchMovies found {Total} results, saved to cache. RequestId: {RequestId}", total, request.RequestId);
        return Ok(result, request.RequestId, "Movies found");
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetById(int id)
    {
        var movie = await movieRepository.GetById(id);

        if (movie is null)
        {
            return base.NotFound(new ApiResponse<MovieResponse>
            {
                Success = false,
                Message = "Movie not found",
                RequestId = string.Empty,  
                ApiVersion = "v3"
            });
        }

        var omdb = await omdbService.GetByIdOrTitleAsync(movie.Title);

        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            AverageRating = 0,
            ReviewCount = 0,
            Runtime = omdb?.Runtime,
            Awards = omdb?.Awards,
            ImdbRating = omdb?.imdbRating,
            RottenTomatoesRating = omdb?.Ratings
                .FirstOrDefault(r => r.Source == "Rotten Tomatoes")?.Value
        };

        return Ok(response, string.Empty, "Movie retrieved");
    }

    [HttpGet("omdb-search")]
    public async Task<ActionResult<ApiResponse<List<OmdbMovieSearchResponse>>>> OmdbSearch(
    [FromQuery] OmdbSearchRequest request)
    {
        if (!ModelState.IsValid)
            return base.BadRequest(new ApiResponse<List<OmdbMovieSearchResponse>>
            {
                Success = false,
                Message = "Invalid parameters",
                RequestId = string.Empty,
                ApiVersion = "v3"
            });

        OmdbSearchResponse? searchResult = await this.omdbService  
            .SearchAsync(request.SearchFilter, request.YearOfRelease);

        if (searchResult is null || searchResult.Search.Count == 0)
            return Ok(new List<OmdbMovieSearchResponse>(), string.Empty, "No results found");

        var results = new List<OmdbMovieSearchResponse>();

        foreach (OmdbSearchItem item in searchResult.Search)
        {
            OmdbMovieResponse? details =
                await this.omdbService.GetByIdOrTitleAsync(item.Title); 

            results.Add(new OmdbMovieSearchResponse
            {
                Title = item.Title,
                ReleaseYear = item.Year,
                Rated = details?.Rated,
                Released = details?.Released,
                Runtime = details?.Runtime,
                Genre = details?.Genre,
                Director = details?.Director,
                Writer = details?.Writer,
                Actors = details?.Actors,
                Description = details?.Plot,
                Awards = details?.Awards,
                ImdbRating = details?.imdbRating,
                RottenTomatoesRating = details?.Ratings
                    .FirstOrDefault(r => r.Source == "Rotten Tomatoes")?.Value
            });
        }

        return Ok(results, string.Empty, "OMDb search completed");
    }

}