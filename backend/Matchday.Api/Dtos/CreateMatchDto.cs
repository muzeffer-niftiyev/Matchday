using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record CreateMatchDto(
    int FieldId,
    DateOnly Date,
    TimeOnly StartTime,
    int HomeTeamId,
    int AwayTeamId,
    [Required(ErrorMessage = ValidationMessages.Required), Length(8, 8, ErrorMessage = ValidationMessages.Lineup)] List<int> PlayerIds
);