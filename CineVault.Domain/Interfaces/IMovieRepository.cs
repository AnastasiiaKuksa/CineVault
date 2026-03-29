
using CineVault.API.Data.Entities;
using CineVault.Domain.Interfaces;

namespace CineVault.API.Data.Interfaces;

public interface IMovieRepository 
{
    Task<IReadOnlyList<Movie>> GetAll();
    Task<Movie?> GetById(int id);
    Task Create(Movie movie);
    Task Update(Movie movie);
    Task Delete(Movie movie);
}
