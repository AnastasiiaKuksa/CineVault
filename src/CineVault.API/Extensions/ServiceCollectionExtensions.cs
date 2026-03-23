using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using CineVault.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCineVaultDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CineVaultDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("CineVaultDb");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured");
            }

            options.UseInMemoryDatabase(connectionString);
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }
}
