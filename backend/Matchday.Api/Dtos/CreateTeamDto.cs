using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record CreateTeamDto(
    [Required(ErrorMessage = ValidationMessages.Required), MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)] string Name,
    [Required(ErrorMessage = ValidationMessages.Required), MaxLength(60, ErrorMessage = ValidationMessages.MaxLength)] string City
);