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
                m.Id, m.Date, m.StartTime, m.Field!.Name, m.HomeTeamId, m.HomeTeam!.Name, m.CreatedAt, m.ExpiresAt
            ))
            .ToListAsync();

            var sent = await db.Matches
            .Where(m => m.Status == MatchStatus.Pending && m.ExpiresAt > now)
            .Where(m => myTeamIds.Contains(m.HomeTeamId))
            .OrderBy(m => m.ExpiresAt)
            .Select(m => new InvitationDto(
                m.Id, m.Date, m.StartTime, m.Field!.Name, m.AwayTeamId, m.AwayTeam!.Name, m.CreatedAt, m.ExpiresAt
            ))
            .ToListAsync();

            return Results.Ok(new MyInvitationsDto(received, sent));
        });
    }
}