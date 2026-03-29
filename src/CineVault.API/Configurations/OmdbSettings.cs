namespace CineVault.API.Configurations;

// Цей клас читає секцію "Omdb" з appsettings.json
public class OmdbSettings
{
    public string ApiKey { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}