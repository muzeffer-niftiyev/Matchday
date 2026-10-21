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
            return "Heyətdə 8 fərqli oyunçu olmalıdır.";
        }

        var membersInLineup = await db.TeamMembers
            .CountAsync(m => m.TeamId == teamId && playerIds.Contains(m.UserId));

        if (membersInLineup != TeamRules.PlayersPerMatch)
        {
            return "Bütün oyunçular komandanızın üzvü olmalıdır.";
        }

        return null;
    }
}