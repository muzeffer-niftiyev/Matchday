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

            var lineupError = await LineupRules.ValidateAsync(db, dto.HomeTeamId, dto.PlayerIds);

            if (lineupError is not null)
            {
                return Results.BadRequest(lineupError);
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

            return Results.Created($"/matches/{match.Id}", MatchDto.From(match));
        }).RequireAuthorization();

        group.MapPost("/{id:int}/accept", async (int id, AcceptMatchDto dto, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var now = DateTime.UtcNow;
            var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == id);

            if (match is null)
            {
                return Results.NotFound("Match not found.");
            }

            var awayTeam = await db.Teams.FirstAsync(t => t.Id == match.AwayTeamId);

            if (awayTeam.OwnerId != userId)
            {
                return Results.Forbid();
            }

            if (match.Status != MatchStatus.Pending)
            {
                return Results.BadRequest("This match is not waiting for an answer.");
            }

            if (match.ExpiresAt <= now)
            {
                match.Status = MatchStatus.Expired;
                await db.SaveChangesAsync();
                return Results.BadRequest("This invitation has expired.");
            }

            var lineupError = await LineupRules.ValidateAsync(db, match.AwayTeamId, dto.PlayerIds);

            if (lineupError is not null)
            {
                return Results.BadRequest(lineupError);
            }

            var playsForHome = await db.MatchPlayers
                .AnyAsync(mp => mp.MatchId == match.Id && dto.PlayerIds.Contains(mp.UserId));

            if (playsForHome)
            {
                return Results.BadRequest("A player can't play for both teams.");
            }

            var teamBusy = await db.Matches
                .Active(now)
                .AnyAsync(m => m.Id != match.Id
                            && m.Date == match.Date
                            && (m.HomeTeamId == match.HomeTeamId || m.AwayTeamId == match.HomeTeamId
                             || m.HomeTeamId == match.AwayTeamId || m.AwayTeamId == match.AwayTeamId));

            if (teamBusy)
            {
                return Results.Conflict("One of the teams already has a game on this day.");
            }

            match.Status = MatchStatus.Confirmed;

            foreach (var playerId in dto.PlayerIds)
            {
                db.MatchPlayers.Add(new MatchPlayer
                {
                    MatchId = match.Id,
                    UserId = playerId,
                    TeamId = match.AwayTeamId
                });
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("This match could not be confirmed. Please try again.");
            }

            return Results.Ok(MatchDto.From(match));

        }).RequireAuthorization();

        group.MapPost("/{id:int}/decline", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var now = DateTime.UtcNow;
            var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == id);

            if (match is null)
            {
                return Results.NotFound("Match not found.");
            }

            var awayTeam = await db.Teams.FirstAsync(t => t.Id == match.AwayTeamId);

            if (awayTeam.OwnerId != userId)
            {
                return Results.Forbid();
            }

            if (match.Status != MatchStatus.Pending)
            {
                return Results.BadRequest("This match is not waiting for an answer.");
            }

            if (match.ExpiresAt <= now)
            {
                match.Status = MatchStatus.Expired;
                await db.SaveChangesAsync();
                return Results.BadRequest("This invitation has expired.");
            }

            match.Status = MatchStatus.Declined;
            await db.SaveChangesAsync();

            return Results.Ok(MatchDto.From(match));
        }).RequireAuthorization();

        group.MapPost("/{id:int}/cancel", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == id);

            if (match is null)
            {
                return Results.NotFound("Match not found.");
            }

            var homeTeam = await db.Teams.FirstAsync(t => t.Id == match.HomeTeamId);

            if (homeTeam.OwnerId != userId)
            {
                return Results.Forbid();
            }

            if (match.Status != MatchStatus.Pending && match.Status != MatchStatus.Confirmed)
            {
                return Results.BadRequest("Only pending or confirmed matches can be cancelled.");
            }

            var matchStart = match.Date.ToDateTime(match.StartTime);

            if (matchStart <= BakuTime.Now)
            {
                return Results.BadRequest("You can't cancel a match that has already started.");
            }

            match.Status = MatchStatus.Cancelled;
            await db.SaveChangesAsync();

            return Results.Ok(MatchDto.From(match));
        }).RequireAuthorization();

        group.MapPost("/{id:int}/score", async (int id, SetScoreDto dto, MatchdayContext db) =>
        {
            var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == id);

            if (match is null)
            {
                return Results.NotFound("Match not found.");
            }

            if (match.Status != MatchStatus.Confirmed && match.Status != MatchStatus.Completed)
            {
                return Results.BadRequest("Only confirmed matches can get a score.");
            }

            var matchEnd = match.Date.ToDateTime(match.StartTime).Add(BookingSlots.Duration);

            if (matchEnd > BakuTime.Now)
            {
                return Results.BadRequest("The match hasn't finished yet.");
            }

            match.HomeScore = dto.HomeScore;
            match.AwayScore = dto.AwayScore;
            match.Status = MatchStatus.Completed;

            await db.SaveChangesAsync();
            return Results.Ok(MatchDto.From(match));

        }).RequireAuthorization("Admin");

        group.MapGet("/{id:int}", async (int id, MatchdayContext db) =>
        {
            var match = await db.Matches
                .Include(m => m.Field)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match is null)
            {
                return Results.NotFound("Match not found.");
            }

            var players = await db.MatchPlayers
                .Where(mp => mp.MatchId == id)
                .Select(mp => new { mp.TeamId, Player = new LineupPlayerDto(mp.UserId, mp.User!.DisplayName) })
                .ToListAsync();

            var homeLineup = players.Where(p => p.TeamId == match.HomeTeamId).Select(p => p.Player).ToList();
            var awayLineup = players.Where(p => p.TeamId == match.AwayTeamId).Select(p => p.Player).ToList();

            var status = match.Status == MatchStatus.Pending && match.ExpiresAt <= DateTime.UtcNow
                ? MatchStatus.Expired
                : match.Status;

            return Results.Ok(new MatchDetailsDto(
                match.Id,
                match.Date,
                match.StartTime,
                match.StartTime.Add(BookingSlots.Duration),
                status,
                match.Field!.Name,
                match.Field.City,
                match.HomeTeamId,
                match.HomeTeam!.Name,
                match.AwayTeamId,
                match.AwayTeam!.Name,
                match.HomeScore,
                match.AwayScore,
                homeLineup,
                awayLineup));
        });
    }
}