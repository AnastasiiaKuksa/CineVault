using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Data.Entities;

public sealed class CineVaultDbContext : DbContext
{
    public required DbSet<Movie> Movies { get; set; }
    public required DbSet<Review> Reviews { get; set; }
    public required DbSet<User> Users { get; set; }
    public required DbSet<Comment> Comments { get; set; }
    public required DbSet<CommentLike> CommentLikes { get; set; }
    public required DbSet<Actor> Actors { get; set; }
    public required DbSet<MovieActor> MovieActors { get; set; }

    public CineVaultDbContext(DbContextOptions<CineVaultDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Soft Delete global filters 
        modelBuilder.Entity<Movie>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<CommentLike>().HasQueryFilter(l => !l.IsDeleted);
        modelBuilder.Entity<Actor>().HasQueryFilter(a => !a.IsDeleted);

        // ── Movie
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(2000);
            entity.Property(m => m.Genre).HasMaxLength(100);
            entity.Property(m => m.Director).HasMaxLength(200);
            // Unique title
            entity.HasIndex(m => m.Title).IsUnique();
        });

        // ── Review 
        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.Comment).HasMaxLength(1000);
            // Unique review per user per movie
            entity.HasIndex(r => new { r.UserId, r.MovieId }).IsUnique();
        });

        // ── User
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Password).IsRequired().HasMaxLength(256);
            // Unique email and username
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        // ── Comment 
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(c => c.Text).HasMaxLength(1000);

            // Fix cascade cycles: User -> Review -> Comment and User -> Comment
            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(c => c.Review)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Actor 
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.Property(a => a.FullName).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Biography).HasMaxLength(3000);
            // Unique full name + birthdate combination
            entity.HasIndex(a => new { a.FullName, a.BirthDate }).IsUnique();
        });

        // ── MovieActor (many-to-many) 
        modelBuilder.Entity<MovieActor>(entity =>
        {
            entity.HasKey(ma => new { ma.MovieId, ma.ActorId });

            entity.HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CommentLike 
        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.HasIndex(l => new { l.UserId, l.CommentId }).IsUnique();

            // Fix cascade cycles: User -> Comment -> CommentLike and User -> CommentLike
            entity.HasOne(l => l.User)
                .WithMany(u => u.CommentLikes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(l => l.Comment)
                .WithMany(c => c.Likes)
                .HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}