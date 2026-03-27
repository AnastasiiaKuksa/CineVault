using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Data.Entities;

public sealed class CineVaultDbContext : DbContext
{
    public required DbSet<Movie> Movies { get; set; }
    public required DbSet<Review> Reviews { get; set; }
    public required DbSet<User> Users { get; set; }
    public required DbSet<Comment> Comments { get; set; }
    public required DbSet<CommentLike> CommentLikes { get; set; }

    public CineVaultDbContext(DbContextOptions<CineVaultDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique review per user per movie
        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.MovieId })
            .IsUnique();

        // Unique like per user per comment
        modelBuilder.Entity<CommentLike>()
            .HasIndex(l => new { l.UserId, l.CommentId })
            .IsUnique();
    }
}