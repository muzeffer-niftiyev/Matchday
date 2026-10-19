using System.ComponentModel.DataAnnotations;
using Matchday.Api.Models;
namespace Matchday.Api.Dtos;

public record CreateFieldDto(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(60)] string City,
    GrassType GrassType,
    [Range(1, 10000)] decimal PricePerHour
);