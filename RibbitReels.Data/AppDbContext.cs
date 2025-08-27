using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RibbitReels.Data.Models;

namespace RibbitReels.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Leaf> Leafs => Set<Leaf>();
    public DbSet<LearningProgress> LearningProgress => Set<LearningProgress>();
    public DbSet<UserBranchAssignment> UserBranchAssignment => Set<UserBranchAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserBranchAssignment>()
            .HasIndex(u => new { u.UserId, u.BranchId })
            .IsUnique();
        modelBuilder.Entity<LearningProgress>()
            .Property(up => up.CompletedLeafIds)
            .HasConversion(
                v => string.Join(",", v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Guid.Parse)
                    .ToList()
            )
            .Metadata.SetValueComparer(
                new ValueComparer<List<Guid>>(
                    (c1, c2) => (c1 ?? new List<Guid>()).SequenceEqual(c2 ?? new List<Guid>()),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                )
            );
    }
}