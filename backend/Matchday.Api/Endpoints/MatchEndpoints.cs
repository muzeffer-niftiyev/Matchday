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
                return Results.BadRequest("Oyunlar ən azı bir gün əvvəl rezerv edilməlidir.");
            }

            var startTimeCheck = BookingSlots.StartTimes.Contains(dto.StartTime);

            if (!startTimeCheck)
            {
                return Results.BadRequest("Yanlış vaxt.");
            }

            if (dto.HomeTeamId == dto.AwayTeamId)
            {
                return Results.BadRequest("Komanda özü ilə oynaya bilməz.");
            }

            var homeTeam = await db.Teams.FirstOrDefaultAsync(t => t.Id == dto.HomeTeamId);
            var awayTeam = await db.Teams.FirstOrDefaultAsync(t => t.Id == dto.AwayTeamId);
            var field = await db.Fields.AnyAsync(f => f.Id == dto.FieldId);

            if (!field)
            {
                return Results.NotFound("Meydança tapılmadı.");
            }

            if (homeTeam is null || awayTeam is null)
            {
                return Results.NotFound("Komandalar tapılmadı.");
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
                return Results.Conflict("Artıq gözləyən dəvətiniz var. Əvvəlcə cavabı gözləyin.");
            }

            var teamBusy = await db.Matches
            .Active(now)
            .AnyAsync(m => m.Date == dto.Date && (
                m.HomeTeamId == dto.HomeTeamId || m.AwayTeamId == dto.AwayTeamId || m.HomeTeamId == dto.AwayTeamId || m.AwayTeamId == dto.HomeTeamId
            ));

            if (teamBusy)
            {
                return Results.Conflict("Komandalardan birinin bu gün artıq oyunu var.");
            }

            var slotTaken = await db.Matches
            .Active(now)
            .AnyAsync(m => m.FieldId == dto.FieldId && m.Date == dto.Date && m.StartTime == dto.StartTime);

            if (slotTaken)
            {
                return Results.Conflict("Bu vaxt artıq tutulub.");
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
                return Results.Conflict("Bu vaxt elə indicə başqası tərəfindən tutuldu.");
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
                return Results.NotFound("Oyun tapılmadı.");
            }

            var awayTeam = await db.Teams.FirstAsync(t => t.Id == match.AwayTeamId);

            if (awayTeam.OwnerId != userId)
            {
                return Results.Forbid();
            }

            if (match.Status != MatchStatus.Pending)
            {
                return Results.BadRequest("Bu oyun cavab gözləmir.");
            }

            if (match.ExpiresAt <= now)
            {
                match.Status = MatchStatus.Expired;
                await db.SaveChangesAsync();
                return Results.BadRequest("Bu dəvətin vaxtı bitib.");
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
                return Results.BadRequest("Oyunçu hər iki komandada oynaya bilməz.");
            }

            var teamBusy = await db.Matches
                .Active(now)
                .AnyAsync(m => m.Id != match.Id
                            && m.Date == match.Date
                            && (m.HomeTeamId == match.HomeTeamId || m.AwayTeamId == match.HomeTeamId
                             || m.HomeTeamId == match.AwayTeamId || m.AwayTeamId == match.AwayTeamId));

            if (teamBusy)
            {
                return Results.Conflict("Komandalardan birinin bu gün artıq oyunu var.");
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
                return Results.Conflict("Oyunu təsdiqləmək mümkün olmadı. Yenidən cəhd edin.");
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
                return Results.NotFound("Oyun tapılmadı.");
            }

            var awayTeam = await db.Teams.FirstAsync(t => t.Id == match.AwayTeamId);

            if (awayTeam.OwnerId != userId)
            {
                return Results.Forbid();
            }

            if (match.Status != MatchStatus.Pending)
            {
                return Results.BadRequest("Bu oyun cavab gözləmir.");
            }

            if (match.ExpiresAt <= now)
            {
                match.Status = MatchStatus.Expired;
                await db.SaveChangesAsync();
                return Results.BadRequest("Bu dəvətin vaxtı bitib.");
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
                return Results.NotFound("Oyun tapılmadı.");
            }

            var homeTeam = await db.Teams.FirstAsync(t => t.Id == match.HomeTeamId);

            if (homeTeam.OwnerId != userId)
            {
                return Results.Forbid();
            }

            if (match.Status != MatchStatus.Pending && match.Status != MatchStatus.Confirmed)
            {
                return Results.BadRequest("Yalnız gözləyən və ya təsdiqlənmiş oyunlar ləğv edilə bilər.");
            }

            var matchStart = match.Date.ToDateTime(match.StartTime);

            if (matchStart <= BakuTime.Now)
            {
                return Results.BadRequest("Başlamış oyunu ləğv edə bilməzsiniz.");
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
                return Results.NotFound("Oyun tapılmadı.");
            }

            if (match.Status != MatchStatus.Confirmed && match.Status != MatchStatus.Completed)
            {
                return Results.BadRequest("Yalnız təsdiqlənmiş oyunlara hesab yazıla bilər.");
            }

            var matchEnd = match.Date.ToDateTime(match.StartTime).Add(BookingSlots.Duration);

            if (matchEnd > BakuTime.Now)
            {
                return Results.BadRequest("Oyun hələ bitməyib.");
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
                return Results.NotFound("Oyun tapılmadı.");
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
                awayLineup,
                match.HomeTeam.Color,
                match.AwayTeam.Color,
                match.HomeTeam.OwnerId,
                match.AwayTeam.OwnerId,
                match.ExpiresAt));
        });

        group.MapGet("/", async (string? when, int? teamId, string? city, MatchdayContext db) =>
        {
            var now = DateTime.UtcNow;
            var today = BakuTime.Today;
            var nowTime = TimeOnly.FromDateTime(BakuTime.Now);

            var query = db.Matches.AsQueryable();

            if (city is not null)
            {
                query = query.Where(m => m.Field!.City == city);
            }

            if (teamId is not null)
            {
                query = query.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);
            }

            if (when == "upcoming")
            {
                query = query
                    .Active(now)
                    .Where(m => m.Date > today || (m.Date == today && m.StartTime > nowTime))
                    .OrderBy(m => m.Date)
                    .ThenBy(m => m.StartTime);
            }
            else if (when == "results")
            {
                query = query
                    .Where(m => m.Status == MatchStatus.Completed)
                    .OrderByDescending(m => m.Date)
                    .ThenByDescending(m => m.StartTime);
            }
            else
            {
                query = query
                    .OrderByDescending(m => m.Date)
                    .ThenByDescending(m => m.StartTime);
            }

            var matches = await query
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
                    m.HomeTeam!.Color,
                    m.AwayTeam.Color
                    ))
                .ToListAsync();

            return Results.Ok(matches);
        });
    }
}