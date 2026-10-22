using Matchday.Api.Models;

namespace Matchday.Api.Dtos;

public record MatchSummaryDto(
    int Id,
    DateOnly Date,
    TimeOnly StartTime,
    MatchStatus Status,
    int HomeTeamId,
    string HomeTeamName,
    int AwayTeamId,
    string AwayTeamName,
    int? HomeScore,
    int? AwayScore,
    string FieldName,
    string FieldCity,
    string HomeTeamColor,
    string AwayTeamColor
);