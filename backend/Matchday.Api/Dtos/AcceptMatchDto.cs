using System.ComponentModel.DataAnnotations;

namespace Matchday.Api.Dtos;

public record AcceptMatchDto(
    [Required(ErrorMessage = ValidationMessages.Required), Length(8, 8, ErrorMessage = ValidationMessages.Lineup)] List<int> PlayerIds
);