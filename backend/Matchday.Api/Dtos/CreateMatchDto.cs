using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record CreateMatchDto(
    int FieldId,
    DateOnly Date,
    TimeOnly StartTime,
    int HomeTeamId,
    int AwayTeamId,
    [Required, Length(8, 8)] List<int> PlayerIds
);