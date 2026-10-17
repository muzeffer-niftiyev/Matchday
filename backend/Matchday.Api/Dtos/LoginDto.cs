using System.ComponentModel.DataAnnotations;

namespace Matchday.Api.Dtos;

public record LoginDto(
    [Required] string Email,
    [Required] string Password
);