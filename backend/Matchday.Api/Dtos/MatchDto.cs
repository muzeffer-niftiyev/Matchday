namespace Matchday.Api.Dtos;

public record MatchDto(
    int Id,
    int FieldId,
    DateOnly Date,
    TimeOnly StartTime,
    int HomeTeamId,
    int AwayTeamId,
    string Status,
    DateTime ExpiresAt
);