using Matchday.Api.Data;
using Matchday.Api.Dtos;
using Matchday.Api.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Matchday.Api.Endpoints;

public static class TeamEndpoints
{
    public static void MapTeamEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/teams");

        group.MapGet("/", async (MatchdayContext db) =>
        {
            var teams = await db.Teams
                .Select(t => new TeamDto(
                    t.Id,
                    t.Name,
                    t.City,
                    t.Color,
                    t.OwnerId,
                    t.Owner!.DisplayName,
                    db.TeamMembers.Count(m => m.TeamId == t.Id),
                    t.CreatedAt
                    ))
                .ToListAsync();

            return Results.Ok(teams);
        });

        group.MapGet("/{id:int}", async (int id, MatchdayContext db) =>
        {
            var team = await db.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamDto(
                    t.Id,
                    t.Name,
                    t.City,
                    t.Color,
                    t.OwnerId,
                    t.Owner!.DisplayName,
                    db.TeamMembers.Count(m => m.TeamId == t.Id),
                    t.CreatedAt
                    ))
                .FirstOrDefaultAsync();

            if (team is null)
            {
                return Results.NotFound("Komanda tapılmadı.");
            }

            return Results.Ok(team);
        });

        group.MapPost("/", async (CreateTeamDto dto, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);

            var alreadyInTeam = await db.TeamMembers.AnyAsync(m => m.UserId == userId);

            if (alreadyInTeam)
            {
                return Results.Conflict("Siz artıq komandadasınız. Yeni komanda yaratmaq üçün əvvəlcə ondan çıxın.");
            }

            var nameTaken = await db.Teams.AnyAsync(t => t.Name == dto.Name);

            if (nameTaken)
            {
                return Results.Conflict("Bu adda komanda artıq mövcuddur.");
            }

            var team = new Team
            {
                Name = dto.Name,
                City = dto.City,
                CreatedAt = DateTime.UtcNow,
                OwnerId = userId,
                Color = dto.Color
            };

            db.Teams.Add(team);

            var membership = new TeamMember
            {
                Team = team,
                UserId = userId,
                Role = TeamRole.Captain,
                JoinedAt = DateTime.UtcNow
            };

            db.TeamMembers.Add(membership);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("Komanda yaradılmadı. Ad artıq tutulmuş ola bilər, ya da siz artıq komandadasınız.");
            }

           
            return Results.Created($"/teams/{team.Id}", new TeamDto(
                    team.Id,
                    team.Name,
                    team.City,
                    team.Color,
                    team.OwnerId,
                    user.FindFirstValue("name")!,
                    1,
                    team.CreatedAt
                    ));
        }).RequireAuthorization();

        group.MapPut("/{id:int}", async (int id, CreateTeamDto dto, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound();
            }

            if (team.OwnerId != userId)
            {
                return Results.Forbid();
            }

            var nameTaken = await db.Teams.AnyAsync(t => t.Name == dto.Name && t.Id != id);

            if (nameTaken)
            {
                return Results.Conflict("Bu adda komanda artıq mövcuddur.");
            }

            team.Name = dto.Name;
            team.City = dto.City;
            team.Color = dto.Color;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("Bu adda komanda artıq mövcuddur.");
            }

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound();
            }

            if (team.OwnerId != userId)
            {
                return Results.Forbid();
            }

            db.Teams.Remove(team);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/{id:int}/members", async (int id, MatchdayContext db) =>
        {
            var teamExists = await db.Teams.AnyAsync(t => t.Id == id);

            if (!teamExists)
            {
                return Results.NotFound();
            }

            var members = await db.TeamMembers
                .Where(m => m.TeamId == id)
                .OrderBy(m => m.JoinedAt)
                .Select(m => new TeamMemberDto(
                    m.UserId,
                    m.User!.DisplayName,
                    m.Role.ToString(),
                    m.JoinedAt))
                .ToListAsync();

            return Results.Ok(members);
        });

        group.MapDelete("/{id:int}/leave", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound();
            }

            if (team.OwnerId == userId)
            {
                return Results.BadRequest("Sahib komandadan çıxa bilməz. Bunun əvəzinə komandanı silin.");
            }

            var membership = await db.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == id && m.UserId == userId);

            if (membership is null)
            {
                return Results.NotFound("Siz bu komandanın üzvü deyilsiniz.");
            }

            db.TeamMembers.Remove(membership);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapDelete("/{id:int}/members/{memberId:int}", async (int id, int memberId, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound();
            }

            if (team.OwnerId != userId)
            {
                return Results.Forbid();
            }

            if (memberId == userId)
            {
                return Results.BadRequest("Özünüzü komandadan çıxara bilməzsiniz.");
            }

            var member = await db.TeamMembers.FirstOrDefaultAsync(m => m.UserId == memberId && m.TeamId == team.Id);

            if (member is null)
            {
                return Results.NotFound("Bu istifadəçi komandanın üzvü deyil.");
            }

            db.TeamMembers.Remove(member);
            await db.SaveChangesAsync();

            return Results.NoContent();

        }).RequireAuthorization();

        group.MapPost("/{id:int}/join-requests", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var teamExists = await db.Teams.AnyAsync(t => t.Id == id);

            if (!teamExists)
            {
                return Results.NotFound("Komanda tapılmadı.");
            }

            var isMember = await db.TeamMembers.AnyAsync(m => m.UserId == userId);

            if (isMember)
            {
                return Results.Conflict("Siz artıq komandadasınız. Əvvəlcə ondan çıxın.");
            }

            var hasRequested = await db.JoinRequests.AnyAsync(r => r.UserId == userId && r.TeamId == id);

            if (hasRequested)
            {
                return Results.Conflict("Bu komandaya artıq sorğu göndərmisiniz.");
            }

            var isTeamFull = await db.TeamMembers.CountAsync(m => m.TeamId == id) >= TeamRules.MaxSquadSize;

            if (isTeamFull)
            {
                return Results.Conflict("Komanda doludur.");
            }

            var joinRequest = new JoinRequest
            {
                TeamId = id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                db.JoinRequests.Add(joinRequest);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("Sorğu göndərilmədi.");
            }

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapDelete("/{id:int}/join-requests", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var request = await db.JoinRequests.FirstOrDefaultAsync(r => r.TeamId == id && r.UserId == userId);

            if (request is null)
            {
                return Results.NotFound("Bu komandaya sorğunuz yoxdur.");
            }

            db.JoinRequests.Remove(request);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/{id:int}/join-requests", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound("Komanda tapılmadı.");
            }

            if (team.OwnerId != userId)
            {
                return Results.Forbid();
            }

            var requests = await db.JoinRequests
            .Where(r => r.TeamId == id)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new JoinRequestDto(r.UserId, r.User!.DisplayName, r.CreatedAt))
            .ToListAsync();

            return Results.Ok(requests);
        }).RequireAuthorization();

        group.MapPost("/{id:int}/join-requests/{memberId:int}/decline", async (int id, int memberId, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound("Komanda tapılmadı.");
            }

            if (team.OwnerId != userId)
            {
                return Results.Forbid();
            }

            var request = await db.JoinRequests.FirstOrDefaultAsync(r => r.TeamId == id && r.UserId == memberId);

            if (request is null)
            {
                return Results.NotFound("Sorğu tapılmadı.");
            }

            db.JoinRequests.Remove(request);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapPost("/{id:int}/join-requests/{memberId:int}/accept", async (int id, int memberId, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound("Komanda tapılmadı.");
            }

            if (team.OwnerId != userId)
            {
                return Results.Forbid();
            }

            var request = await db.JoinRequests.FirstOrDefaultAsync(r => r.TeamId == id && r.UserId == memberId);

            if (request is null)
            {
                return Results.NotFound("Sorğu tapılmadı.");
            }

            var isTeamFull = await db.TeamMembers.CountAsync(m => m.TeamId == id) >= TeamRules.MaxSquadSize;

            if (isTeamFull)
            {
                return Results.Conflict("Komanda doludur.");
            }

            var player = new TeamMember
            {
                Team = team,
                UserId = memberId,
                Role = TeamRole.Player,
                JoinedAt = DateTime.UtcNow
            };

            db.TeamMembers.Add(player);

            var allRequests = await db.JoinRequests
            .Where(r => r.UserId == memberId)
            .ToListAsync();

            db.JoinRequests.RemoveRange(allRequests);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("Bu oyunçu artıq başqa komandaya qoşulub.");
            }

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/{id:int}/matches", async (int id, MatchdayContext db) =>
        {
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound();
            }

            var matches = await db.Matches
               .Where(m => m.HomeTeamId == id || m.AwayTeamId == id)
               .OrderByDescending(m => m.Date)
               .ThenByDescending(m => m.StartTime)
               .Select(m => new MatchSummaryDto(
                   m.Id,
                   m.Date,
                   m.StartTime,
                   m.Status,
                   m.HomeTeamId,
                   m.HomeTeam!.Name,
                   m.AwayTeamId,
                   m.AwayTeam!.Name,
                   m.HomeScore,
                   m.AwayScore,
                   m.Field!.Name,
                   m.Field.City,
                   m.HomeTeam!.Color,
                   m.AwayTeam!.Color
                   ))
               .ToListAsync();

            return Results.Ok(matches);
        });
    }
}