using Microsoft.EntityFrameworkCore;
using SolicitorScraper.Domain.Entities;

namespace SolicitorScraper.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<SearchRun> SearchRuns => Set<SearchRun>();
    public DbSet<SolicitorResult> SolicitorResults => Set<SolicitorResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(l => l.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(l => l.Name).IsUnique();
            entity.HasData(
                new Location { Id = 1, Name = "London" },
                new Location { Id = 2, Name = "Birmingham" },
                new Location { Id = 3, Name = "Leeds" },
                new Location { Id = 4, Name = "Manchester" },
                new Location { Id = 5, Name = "Sheffield" },
                new Location { Id = 6, Name = "Bradford" },
                new Location { Id = 7, Name = "Liverpool" },
                new Location { Id = 8, Name = "Bristol" });
        });

        modelBuilder.Entity<SearchRun>(entity =>
        {
            entity.HasMany(r => r.Results)
                .WithOne()
                .HasForeignKey(s => s.SearchRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitorResult>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(255).IsRequired();
            entity.Property(s => s.Location).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Phone).HasMaxLength(50);
            entity.Property(s => s.Address).HasMaxLength(500);
            entity.Property(s => s.Website).HasMaxLength(500);
            entity.Property(s => s.ProfileUrl).HasMaxLength(500);
            entity.Property(s => s.QualityMarks).HasMaxLength(500);
            entity.Property(s => s.Rating).HasPrecision(2, 1);
            entity.HasIndex(s => new { s.SearchRunId, s.Location });
        });
    }
}
