namespace Matchday.Api.Dtos;

public record InvitationDto(
    int MatchId,
    DateOnly Date,
    TimeOnly StartTime,
    string FieldName,
    int OpponentTeamId,
    string OpponentTeamName,
    DateTime CreatedAt,
    DateTime ExpiresAt
);