using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Data.Repositories;

public sealed class MovieRepository : IMovieRepository
{
    private readonly CineVaultDbContext cineVaultDbContext;

    public MovieRepository(CineVaultDbContext cineVaultDbContext)
    {
        this.cineVaultDbContext = cineVaultDbContext;
    }

    public async Task<IReadOnlyList<Movie>> GetAll()
    {
        return await this.cineVaultDbContext.Movies
            .Include(m => m.Reviews)
            .ToListAsync();
    }

    public async Task<Movie?> GetById(int id)
    {
        return await this.cineVaultDbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task Create(Movie movie)
    {
        await this.cineVaultDbContext.Movies.AddAsync(movie);
        await this.cineVaultDbContext.SaveChangesAsync();
    }

    public async Task Update(Movie movie)
    {
        this.cineVaultDbContext.Movies.Update(movie);
        await this.cineVaultDbContext.SaveChangesAsync();
    }

    public async Task Delete(Movie movie)
    {
        this.cineVaultDbContext.Movies.Remove(movie);
        await this.cineVaultDbContext.SaveChangesAsync();
    }
}