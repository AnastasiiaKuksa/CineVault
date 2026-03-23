using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Data.Entities;

public sealed class CineVaultDbContext : DbContext
{
    public required DbSet<Movie> Movies { get; set; }
    public required DbSet<Review> Reviews { get; set; }
    public required DbSet<User> Users { get; set; }

    public CineVaultDbContext(DbContextOptions<CineVaultDbContext> options)
        : base(options)
    {
    }
}