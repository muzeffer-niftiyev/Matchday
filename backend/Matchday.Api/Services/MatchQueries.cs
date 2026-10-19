using Matchday.Api.Models;
namespace Matchday.Api.Services;

public static class MatchQueries
{
    public static IQueryable<Match> Active(this IQueryable<Match> matches, DateTime now) =>
        matches.Where(m => m.Status == MatchStatus.Confirmed
                        || (m.Status == MatchStatus.Pending && m.ExpiresAt > now));
}