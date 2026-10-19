namespace Matchday.Api.Data;

using Microsoft.EntityFrameworkCore;
using Matchday.Api.Models;

public class MatchdayContext(DbContextOptions<MatchdayContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();

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

        modelBuilder.Entity<Field>()
            .Property(f => f.PricePerHour)
            .HasPrecision(8, 2);

        modelBuilder.Entity<Match>()
            .HasIndex(m => new { m.FieldId, m.Date, m.StartTime })
            .IsUnique()
            .HasFilter("\"Status\" IN (0, 1)");

        modelBuilder.Entity<MatchPlayer>()
            .HasKey(mp => new { mp.MatchId, mp.UserId });
    }
}