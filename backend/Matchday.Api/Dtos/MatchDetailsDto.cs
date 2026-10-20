using Matchday.Api.Models;

namespace Matchday.Api.Dtos;

public record MatchDetailsDto(
    int Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    MatchStatus Status,
    string FieldName,
    string FieldCity,
    int HomeTeamId,
    string HomeTeamName,
    int AwayTeamId,
    string AwayTeamName,
    int? HomeScore,
    int? AwayScore,
    List<LineupPlayerDto> HomeLineup,
    List<LineupPlayerDto> AwayLineup
);