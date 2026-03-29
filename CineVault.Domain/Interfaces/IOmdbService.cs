using CineVault.API.Responses.Omdb;

namespace CineVault.API.Data.Interfaces;

// Інтерфейс — це "контракт": визначає ЩО робить сервіс, але не ЯК
public interface IOmdbService
{
    // Шукає фільм за назвою або imdb ID
    Task<OmdbMovieResponse?> GetByIdOrTitleAsync(string idOrTitle);

    // Шукає список фільмів за пошуковим запитом
    Task<OmdbSearchResponse?> SearchAsync(string search, int? year = null);
}