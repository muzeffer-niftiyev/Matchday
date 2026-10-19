using Matchday.Api.Data;
using Matchday.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Matchday.Api.Services;

public static class LineupRules
{
    public static async Task<string?> ValidateAsync(MatchdayContext db, int teamId, List<int> playerIds)
    {
        if (playerIds.Distinct().Count() != TeamRules.PlayersPerMatch)
        {
            return "The lineup must have 8 different players.";
        }

        var membersInLineup = await db.TeamMembers
            .CountAsync(m => m.TeamId == teamId && playerIds.Contains(m.UserId));

        if (membersInLineup != TeamRules.PlayersPerMatch)
        {
            return "All players must be members of your team.";
        }

        return null;
    }
}