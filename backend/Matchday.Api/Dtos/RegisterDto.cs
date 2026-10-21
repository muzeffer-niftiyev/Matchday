using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record RegisterDto(
    [Required(ErrorMessage = ValidationMessages.Required), EmailAddress(ErrorMessage = ValidationMessages.Email), MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)] string Email,
    [Required(ErrorMessage = ValidationMessages.Required), MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)] string DisplayName,
    [Required(ErrorMessage = ValidationMessages.Required), MinLength(8, ErrorMessage = ValidationMessages.MinLength), MaxLength(60, ErrorMessage = ValidationMessages.MaxLength)] string Password
);