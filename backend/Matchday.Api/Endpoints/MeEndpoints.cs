using System.Security.Claims;
using Matchday.Api.Data;
using Matchday.Api.Dtos;
using Matchday.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Matchday.Api.Endpoints;

public static class MeEndpoints
{
    public static void MapMeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/me").RequireAuthorization();

        group.MapGet("/invitations", async (MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var now = DateTime.UtcNow;

            var myTeamIds = await db.Teams
            .Where(t => t.OwnerId == userId)
            .Select(t => t.Id)
            .ToListAsync();

            var received = await db.Matches
            .Where(m => m.Status == MatchStatus.Pending && m.ExpiresAt > now)
            .Where(m => myTeamIds.Contains(m.AwayTeamId))
            .OrderBy(m => m.ExpiresAt)
            .Select(m => new InvitationDto(
                m.Id, m.Date, m.StartTime, m.Field!.Name, m.HomeTeamId, m.HomeTeam!.Name, m.CreatedAt, m.ExpiresAt, m.HomeTeam.Color
            ))
            .ToListAsync();

            var sent = await db.Matches
            .Where(m => m.Status == MatchStatus.Pending && m.ExpiresAt > now)
            .Where(m => myTeamIds.Contains(m.HomeTeamId))
            .OrderBy(m => m.ExpiresAt)
            .Select(m => new InvitationDto(
                m.Id, m.Date, m.StartTime, m.Field!.Name, m.AwayTeamId, m.AwayTeam!.Name, m.CreatedAt, m.ExpiresAt, m.AwayTeam.Color
            ))
            .ToListAsync();

            return Results.Ok(new MyInvitationsDto(received, sent));
        });

        group.MapGet("/team", async (MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var teamMember = await db.TeamMembers.FirstOrDefaultAsync(m => m.UserId == userId);

            if (teamMember is null)
            {
                return Results.NoContent();
            }

            var team = await db.Teams
            .Where(t => t.Id == teamMember.TeamId)
            .Select(t => new TeamDto(
                t.Id,
                t.Name,
                t.City,
                t.Color,
                t.OwnerId,
                t.Owner!.DisplayName,
                db.TeamMembers.Count(m => m.TeamId == t.Id),
                t.CreatedAt
            )).FirstOrDefaultAsync();

            return Results.Ok(team);
        });

        group.MapGet("/join-requests", async (MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);

            // r.Team inside Select is fine: EF turns it into a JOIN in SQL
            var requests = await db.JoinRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new MyJoinRequestDto(r.TeamId, r.Team!.Name, r.Team.City, r.Team.Color, r.CreatedAt))
                .ToListAsync();

            return Results.Ok(requests);
        });

        // Matches I'm in the lineup of (home or away), newest first
        group.MapGet("/matches", async (MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var now = DateTime.UtcNow;

            var matches = await db.Matches
                .Where(m => db.MatchPlayers.Any(mp => mp.MatchId == m.Id && mp.UserId == userId))
                .OrderByDescending(m => m.Date)
                .ThenByDescending(m => m.StartTime)
                .Select(m => new MatchSummaryDto(
                    m.Id,
                    m.Date,
                    m.StartTime,
                    // Lazy expiry: a Pending invitation older than 5h is shown as Expired
                    m.Status == MatchStatus.Pending && m.ExpiresAt <= now ? MatchStatus.Expired : m.Status,
                    m.HomeTeamId,
                    m.HomeTeam!.Name,
                    m.AwayTeamId,
                    m.AwayTeam!.Name,
                    m.HomeScore,
                    m.AwayScore,
                    m.Field!.Name,
                    m.Field.City,
                    m.HomeTeam.Color,
                    m.AwayTeam.Color))
                .ToListAsync();

            return Results.Ok(matches);
        });
    }
}