using CineVault.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.BackgroundServices;

public sealed class OldMoviesCleanerService(
    IServiceScopeFactory scopeFactory,
    ILogger<OldMoviesCleanerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OldMoviesCleanerService started");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(45));

        await CleanOldMoviesAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanOldMoviesAsync(stoppingToken);
        }
    }

    private async Task CleanOldMoviesAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CineVaultDbContext>();

        try
        {
            var twoYearsAgo = DateTime.UtcNow.AddYears(-2);
            var threeYearsAgo = DateTime.UtcNow.AddYears(-3);

            var oldMovies = await context.Movies
                .Where(m => !m.IsDeleted &&
                    (
                        (!m.Reviews.Any() &&
                            m.ReleaseDate.HasValue &&
                            m.ReleaseDate.Value < DateOnly.FromDateTime(threeYearsAgo))
                        ||
                        (m.Reviews.Any() &&
                            m.Reviews.Max(r => r.CreatedAt) < twoYearsAgo)
                    ))
                .ToListAsync(ct);

            if (oldMovies.Count == 0)
            {
                logger.LogInformation("OldMoviesCleaner: no outdated movies found at {Time}", DateTime.UtcNow);
                return;
            }

            foreach (var movie in oldMovies)
            {
                movie.IsDeleted = true;
            }

            await context.SaveChangesAsync(ct);

            logger.LogInformation(
                "Old movies cleaned: {Deleted} soft-deleted at {Time}",
                oldMovies.Count, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error cleaning old movies");
        }
    }
}