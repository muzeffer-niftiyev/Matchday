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
                .Select(t => new TeamDto(t.Id, t.Name, t.City, t.CreatedAt))
                .ToListAsync();

            return Results.Ok(teams);
        });

        group.MapGet("/{id:int}", async (int id, MatchdayContext db) =>
        {
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new TeamDto(team.Id, team.Name, team.City, team.CreatedAt));
        });

        group.MapPost("/", async (CreateTeamDto dto, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);

            var alreadyInTeam = await db.TeamMembers.AnyAsync(m => m.UserId == userId);

            if (alreadyInTeam)
            {
                return Results.Conflict("You're already in a team. Leave it first to create a new one.");
            }

            var nameTaken = await db.Teams.AnyAsync(t => t.Name == dto.Name);

            if (nameTaken)
            {
                return Results.Conflict("A team with this name already exists.");
            }

            var team = new Team
            {
                Name = dto.Name,
                City = dto.City,
                CreatedAt = DateTime.UtcNow,
                OwnerId = userId
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
                return Results.Conflict("The team could not be created. The name may be taken, or you're already in a team.");
            }

            return Results.Created($"/teams/{team.Id}", new TeamDto(team.Id, team.Name, team.City, team.CreatedAt));
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
                return Results.Conflict("A team with this name already exists.");
            }

            team.Name = dto.Name;
            team.City = dto.City;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("A team with this name already exists.");
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

        group.MapPost("/{id:int}/join", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var teamExists = await db.Teams.AnyAsync(t => t.Id == id);

            if (!teamExists)
            {
                return Results.NotFound();
            }

            var alreadyMember = await db.TeamMembers.AnyAsync(m => m.TeamId == id && m.UserId == userId);

            if (alreadyMember)
            {
                return Results.Conflict("You are already a member of this team.");
            }

            var member = new TeamMember
            {
                TeamId = id,
                UserId = userId,
                Role = TeamRole.Player,
                JoinedAt = DateTime.UtcNow
            };

            try
            {
                db.TeamMembers.Add(member);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("You are already a member of this team.");
            }

            return Results.NoContent();
        }).RequireAuthorization();

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
                return Results.BadRequest("The owner cannot leave the team.Delete the team instead.");
            }

            var membership = await db.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == id && m.UserId == userId);

            if (membership is null)
            {
                return Results.NotFound("You are not a member of this team.");
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
                return Results.BadRequest("You cannot remove yourself.");
            }

            var member = await db.TeamMembers.FirstOrDefaultAsync(m => m.UserId == memberId && m.TeamId == team.Id);

            if (member is null)
            {
                return Results.NotFound("This user is not a member of this team.");
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
                return Results.NotFound("Team not found.");
            }

            var isMember = await db.TeamMembers.AnyAsync(m => m.UserId == userId);

            if (isMember)
            {
                return Results.Conflict("You're already in a team.");
            }

            var hasRequested = await db.JoinRequests.AnyAsync(r => r.UserId == userId && r.TeamId == id);

            if (hasRequested)
            {
                return Results.Conflict("You already requested to join this team.");
            }

            var isTeamFull = await db.TeamMembers.CountAsync(m => m.TeamId == id) >= TeamRules.MaxSquadSize;

            if (isTeamFull)
            {
                return Results.Conflict("The team is full.");
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
                return Results.Conflict("Request couldn't send.");
            }

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapDelete("/{id:int}/join-requests", async (int id, MatchdayContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue("sub")!);
            var request = await db.JoinRequests.FirstOrDefaultAsync(r => r.TeamId == id && r.UserId == userId);

            if (request is null)
            {
                return Results.NotFound("You have no request for this team.");
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
                return Results.NotFound("Team not found.");
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
    }
}