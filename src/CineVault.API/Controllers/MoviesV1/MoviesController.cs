using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers.MoviesV1;

[ApiVersion(1.0, Deprecated = true)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public sealed class MoviesController : ControllerBase
{
    private readonly IMovieRepository movieRepository;
    private readonly ILogger<MoviesController> logger;

    public MoviesController(IMovieRepository movieRepository, ILogger<MoviesController> logger)
    {
        this.movieRepository = movieRepository;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieResponse>>> GetMovies()
    {
        this.logger.LogInformation("Fetching all movies from the database.");
        var movies = await this.movieRepository.GetAll();
        var response = movies.Select(MovieResponse.FromEntity);
        this.logger.LogInformation("Retrieved {MovieCount} movies successfully.", response.Count());
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieResponse>> GetMovieById(int id)
    {
        this.logger.LogInformation("Movie {MovieId} requested.", id);
        var movie = await this.movieRepository.GetById(id);
        if (movie is null)
        {
            this.logger.LogWarning("Movie {MovieId} not found.", id);
            return NotFound();
        }
        this.logger.LogInformation("Movie {MovieId} retrieved successfully.", id);
        return Ok(MovieResponse.FromEntity(movie));
    }

    [HttpPost]
    public async Task<ActionResult> CreateMovie(MovieRequest request)
    {
        this.logger.LogInformation("Creating a new movie with title {MovieTitle}.", request.Title);
        var movie = request.ToEntity();
        await this.movieRepository.Create(movie);
        this.logger.LogInformation("Movie {MovieTitle} created successfully.", request.Title);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateMovie(int id, MovieRequest request)
    {
        this.logger.LogInformation("Updating movie {MovieId}.", id);
        var movie = await this.movieRepository.GetById(id);
        if (movie is null)
        {
            this.logger.LogWarning("Movie {MovieId} not found for update.", id);
            return NotFound();
        }
        request.ApplyTo(movie);
        await this.movieRepository.Update(movie);
        this.logger.LogInformation("Movie {MovieId} updated successfully.", id);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        this.logger.LogInformation("Deleting movie {MovieId}.", id);
        var movie = await movieRepository.GetById(id);
        if (movie is null)
        {
            this.logger.LogWarning("Movie {MovieId} not found for deletion.", id);
            return NotFound();
        }
        await this.movieRepository.Delete(movie);
        this.logger.LogInformation("Movie {MovieId} deleted successfully.", id);
        return NoContent();
    }
}