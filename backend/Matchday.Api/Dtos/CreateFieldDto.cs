using System.ComponentModel.DataAnnotations;
using Matchday.Api.Models;
namespace Matchday.Api.Dtos;

public record CreateFieldDto(
    [Required(ErrorMessage = ValidationMessages.Required), MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)] string Name,
    [Required(ErrorMessage = ValidationMessages.Required), MaxLength(60, ErrorMessage = ValidationMessages.MaxLength)] string City,
    GrassType GrassType,
    [Range(1, 10000, ErrorMessage = ValidationMessages.Range)] decimal PricePerHour
);