using System.ComponentModel.DataAnnotations;

namespace Matchday.Api.Dtos;

public record LoginDto(
    [Required(ErrorMessage = ValidationMessages.Required)] string Email,
    [Required(ErrorMessage = ValidationMessages.Required)] string Password
);