using System.Text.Json;
using CineVault.API.Configurations;
using CineVault.API.Data.Interfaces;
using CineVault.API.Responses.Omdb;
using Microsoft.Extensions.Options;

namespace CineVault.API.Services;

// OmdbService — клас, який реально робить HTTP запити до omdbapi.com
public class OmdbService(
    HttpClient httpClient,
    IOptions<OmdbSettings> options,
    ILogger<OmdbService> logger) : IOmdbService
{
    private readonly OmdbSettings _settings = options.Value;

    // Метод 1: пошук за назвою — використовує GetAsync (простіший спосіб)
    public async Task<OmdbMovieResponse?> GetByIdOrTitleAsync(string idOrTitle)
    {
        // Будуємо URL: ?t=Inception&type=movie&plot=short&r=json&apikey=...
        string url = $"?t={Uri.EscapeDataString(idOrTitle)}" +
                     $"&type=movie&plot=short&r=json&apikey={_settings.ApiKey}";

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("OMDb GetByTitle failed. Status: {Status}", response.StatusCode);
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            logger.LogInformation("OMDb GetByTitle response: {Json}", json);

            return JsonSerializer.Deserialize<OmdbMovieResponse>(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception in GetByIdOrTitleAsync for: {Title}", idOrTitle);
            return null;
        }
    }

    // Метод 2: пошук за ключовим словом — використовує SendAsync (гнучкіший спосіб)
    public async Task<OmdbSearchResponse?> SearchAsync(string search, int? year = null)
    {
        // Будуємо URL з опціональним роком
        string url = $"?s={Uri.EscapeDataString(search)}" +
                     $"&type=movie&r=json&apikey={_settings.ApiKey}";

        if (year.HasValue)
            url += $"&y={year.Value}";

        try
        {
            // SendAsync — дає більше контролю над запитом (можна додати headers тощо)
            var request = new HttpRequestMessage(HttpMethod.Get,
                _settings.BaseUrl + url);
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("OMDb Search failed. Status: {Status}", response.StatusCode);
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            logger.LogInformation("OMDb Search response: {Json}", json);

            return JsonSerializer.Deserialize<OmdbSearchResponse>(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception in SearchAsync for: {Search}", search);
            return null;
        }
    }
}