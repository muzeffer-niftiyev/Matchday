using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record SetScoreDto(
    [Range(0, 99, ErrorMessage = ValidationMessages.Range)] int HomeScore,
    [Range(0, 99, ErrorMessage = ValidationMessages.Range)] int AwayScore
);