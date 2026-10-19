using Matchday.Api.Data;
using Matchday.Api.Dtos;
using Matchday.Api.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Matchday.Api.Services;

namespace Matchday.Api.Endpoints;

public static class MatchEndpoints
{
    public static void MapMatchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/matches");

        group.MapPost("/", async (CreateMatchDto dto, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);

            if (dto.Date <= BakuTime.Today)
            {
                return Results.BadRequest("Date must be tomorrow or later");
            }

            var startTimeCheck = BookingSlots.StartTimes.Contains(dto.StartTime);

            if (!startTimeCheck)
            {
                return Results.BadRequest("Invalid slot");
            }

            if (dto.HomeTeamId == dto.AwayTeamId)
            {
                return Results.BadRequest("Home and away team can't be the same team");
            }

            var homeTeam = await db.Teams.FirstOrDefaultAsync(t => t.Id == dto.HomeTeamId);
            var awayTeam = await db.Teams.FirstOrDefaultAsync(t => t.Id == dto.AwayTeamId);
            var field = await db.Fields.AnyAsync(f => f.Id == dto.FieldId);

            if (!field)
            {
                return Results.NotFound("Field not found.");
            }

            if (homeTeam is null || awayTeam is null)
            {
                return Results.NotFound("Teams not found.");
            }

            if (homeTeam.OwnerId != userId)
            {
                return Results.Forbid();
            }

            var now = DateTime.UtcNow;

            if (dto.PlayerIds.Distinct().Count() != TeamRules.PlayersPerMatch)
            {
                return Results.BadRequest("The lineup must have 8 different players.");
            }

            var membersInLineup = await db.TeamMembers
            .CountAsync(m => m.TeamId == dto.HomeTeamId && dto.PlayerIds.Contains(m.UserId));

            if (membersInLineup != TeamRules.PlayersPerMatch)
            {
                return Results.BadRequest("All players must be members of your team.");
            }

            var hasPendingInvite = await db.Matches
            .Active(now)
            .AnyAsync(m => m.HomeTeamId == dto.HomeTeamId && m.Status == MatchStatus.Pending);

            if (hasPendingInvite)
            {
                return Results.Conflict("You already have a pending invitation. Wait for an answer first.");
            }

            var teamBusy = await db.Matches
            .Active(now)
            .AnyAsync(m => m.Date == dto.Date && (
                m.HomeTeamId == dto.HomeTeamId || m.AwayTeamId == dto.AwayTeamId || m.HomeTeamId == dto.AwayTeamId || m.AwayTeamId == dto.HomeTeamId
            ));

            if (teamBusy)
            {
                return Results.Conflict("One of the teams already has a game on this day.");
            }

            var slotTaken = await db.Matches
            .Active(now)
            .AnyAsync(m => m.FieldId == dto.FieldId && m.Date == dto.Date && m.StartTime == dto.StartTime);

            if (slotTaken)
            {
                return Results.Conflict("This slot is already taken.");
            }

            var match = new Match
            {
                FieldId = dto.FieldId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                HomeTeamId = dto.HomeTeamId,
                AwayTeamId = dto.AwayTeamId,
                Status = MatchStatus.Pending,
                CreatedAt = now,
                ExpiresAt = now.Add(TeamRules.InvitationExpiry)
            };
            db.Matches.Add(match);

            foreach (var playerId in dto.PlayerIds)
            {
                db.MatchPlayers.Add(new MatchPlayer
                {
                    Match = match,
                    UserId = playerId,
                    TeamId = dto.HomeTeamId
                });
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("This slot was just taken by someone else.");
            }

            return Results.Created($"/matches/{match.Id}", new MatchDto(
                match.Id, match.FieldId, match.Date, match.StartTime,
                match.HomeTeamId, match.AwayTeamId, match.Status.ToString(), match.ExpiresAt));
        }).RequireAuthorization();
    }
}