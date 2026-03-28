using CineVault.API.Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.BackgroundServices;

public sealed class MovieStatsUpdaterService(
    IServiceScopeFactory scopeFactory,
    ILogger<MovieStatsUpdaterService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MovieStatsUpdaterService started");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        await UpdateStatsAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateStatsAsync(stoppingToken);
        }
    }

    private async Task UpdateStatsAsync(CancellationToken ct)
    {

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CineVaultDbContext>();

        try
        {
            var freshStats = await context.Movies
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Select(m => new
                {
                    m.Id,
                    m.IsDeleted,
                    AverageRating = m.Reviews.Any()
                        ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 2)
                        : 0.0,
                    ReviewsCount = m.Reviews.Count(r => !r.IsDeleted),
                    CommentsCount = m.Reviews
                        .Where(r => !r.IsDeleted)
                        .SelectMany(r => r.Comments)
                        .Count(c => !c.IsDeleted)
                })
                .ToListAsync(ct);

            var existingDict = await context.MovieStatistics
                .ToDictionaryAsync(s => s.MovieId, ct);

            int newCount = 0, updatedCount = 0;

            foreach (var stat in freshStats)
            {
                if (!existingDict.TryGetValue(stat.Id, out var existing))
                {
                    context.MovieStatistics.Add(new MovieStatistic
                    {
                        MovieId = stat.Id,
                        AverageRating = stat.AverageRating,
                        ReviewsCount = stat.ReviewsCount,
                        CommentsCount = stat.CommentsCount,
                        IsDeleted = stat.IsDeleted,
                        LastUpdated = DateTime.UtcNow
                    });
                    newCount++;
                }
                else if (existing.AverageRating != stat.AverageRating
                      || existing.ReviewsCount != stat.ReviewsCount
                      || existing.CommentsCount != stat.CommentsCount
                      || existing.IsDeleted != stat.IsDeleted)
                {
                    existing.AverageRating = stat.AverageRating;
                    existing.ReviewsCount = stat.ReviewsCount;
                    existing.CommentsCount = stat.CommentsCount;
                    existing.IsDeleted = stat.IsDeleted;
                    existing.LastUpdated = DateTime.UtcNow;
                    updatedCount++;
                }
            }

            await context.SaveChangesAsync(ct);

            logger.LogInformation(
                "Movie stats updated: {New} new, {Updated} updated at {Time}",
                newCount, updatedCount, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error updating movie statistics");
        }
    }
}