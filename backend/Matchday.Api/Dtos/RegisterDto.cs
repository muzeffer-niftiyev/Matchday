using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record RegisterDto(
    [Required, EmailAddress, MaxLength(100)] string Email,
    [Required, MaxLength(50)] string DisplayName,
    [Required, MinLength(8), MaxLength(60)] string Password
);