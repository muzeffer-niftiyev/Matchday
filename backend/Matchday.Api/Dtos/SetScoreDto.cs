using System.ComponentModel.DataAnnotations;
namespace Matchday.Api.Dtos;

public record SetScoreDto(
    [Range(0, 99)] int HomeScore,
    [Range(0, 99)] int AwayScore
);