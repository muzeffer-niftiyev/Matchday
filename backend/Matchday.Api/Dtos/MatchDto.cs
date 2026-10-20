using Matchday.Api.Models;

namespace Matchday.Api.Dtos;

public record MatchDto(
    int Id,
    int FieldId,
    DateOnly Date,
    TimeOnly StartTime,
    int HomeTeamId,
    int AwayTeamId,
    string Status,
    DateTime ExpiresAt,
    int? HomeScore,
    int? AwayScore
)
{
    public static MatchDto From(Match m) => new(
        m.Id, m.FieldId, m.Date, m.StartTime,
        m.HomeTeamId, m.AwayTeamId, m.Status.ToString(), m.ExpiresAt,
        m.HomeScore, m.AwayScore);
}