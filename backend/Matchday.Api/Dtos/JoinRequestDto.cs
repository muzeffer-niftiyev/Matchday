namespace Matchday.Api.Dtos;

public record JoinRequestDto(
    int UserId,
    string DisplayName,
    DateTime CreatedAt
);