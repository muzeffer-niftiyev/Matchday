using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record CreateTeamDto(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(60)] string City
);