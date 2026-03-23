using CineVault.API.Data.Entities;
using CineVault.API.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Data.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly CineVaultDbContext cineVaultDbContext;

    public UserRepository(CineVaultDbContext cineVaultDbContext)
    {
        this.cineVaultDbContext = cineVaultDbContext;
    }

    public async Task<IReadOnlyList<User>> GetAll()
    {
        return await this.cineVaultDbContext.Users.ToListAsync();
    }

    public async Task<User?> GetById(int id)
    {
        return await this.cineVaultDbContext.Users.FindAsync(id);
    }

    public async Task Create(User user)
    {
        await this.cineVaultDbContext.Users.AddAsync(user);
        await this.cineVaultDbContext.SaveChangesAsync();
    }

    public async Task Update(User user)
    {
        this.cineVaultDbContext.Users.Update(user);
        await this.cineVaultDbContext.SaveChangesAsync();
    }

    public async Task Delete(User user)
    {
        this.cineVaultDbContext.Users.Remove(user);
        await this.cineVaultDbContext.SaveChangesAsync();
    }
}