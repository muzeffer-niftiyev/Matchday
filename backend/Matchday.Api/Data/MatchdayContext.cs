namespace Matchday.Api.Data;

using Microsoft.EntityFrameworkCore;
using Matchday.Api.Models;

public class MatchdayContext(DbContextOptions<MatchdayContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<TeamMember>()
            .HasKey(m => new { m.TeamId, m.UserId });
    }
}