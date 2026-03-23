using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Entities;

namespace CineVault.API.Data.Interfaces;

public interface IMovieRepository
{
    Task<IReadOnlyList<Movie>> GetAll();
    Task<Movie?> GetById(int id);
    Task Create(Movie movie);
    Task Update(Movie movie);
    Task Delete(Movie movie);
}
