using Asp.Versioning;
using CineVault.API.Common.Mappings;
using CineVault.API.Configurations;
using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using CineVault.API.Data.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

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

    public static IServiceCollection AddApiVersioningWithApiExplorer(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        return services;
    }

    public static IServiceCollection AddSwaggerWithOptions(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        return services;
    }

    public static IServiceCollection AddMapster(this IServiceCollection services, Type assemblyMarker)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assemblyMarker.Assembly);

        var mappingConfig = new MappingConfig();
        mappingConfig.Register(config);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}