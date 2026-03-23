using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Data.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly CineVaultDbContext cineVaultDbContext;

    public ReviewRepository(CineVaultDbContext cineVaultDbContext)
    {
        this.cineVaultDbContext = cineVaultDbContext;
    }

    public async Task<IReadOnlyList<Review>> GetAllWithDetails()
    {
        return await this.cineVaultDbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdWithDetails(int id)
    {
        return await this.cineVaultDbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task Create(Review review)
    {
        await this.cineVaultDbContext.Reviews.AddAsync(review);
        await this.cineVaultDbContext.SaveChangesAsync();
    }

    public async Task Update(Review review)
    {
        this.cineVaultDbContext.Reviews.Update(review);
        await this.cineVaultDbContext.SaveChangesAsync();
    }

    public async Task Delete(Review review)
    {
        this.cineVaultDbContext.Reviews.Remove(review);
        await this.cineVaultDbContext.SaveChangesAsync();
    }
}
