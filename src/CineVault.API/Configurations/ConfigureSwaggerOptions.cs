using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CineVault.API.Configurations;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IServiceProvider provider;

    public ConfigureSwaggerOptions(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        using var scope = this.provider.CreateScope();
        var DescriptionProvider = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in DescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "CineVault API",
                    Version = description.ApiVersion.ToString(),
                    Description = description.IsDeprecated
                        ? "<span style=\"color: red; font-weight: bold;\">⚠️ DEPRECATED</span> — This version is no longer supported. Use the latest version."
                        : "The current stable version of the API."
                });
        }
    }
}