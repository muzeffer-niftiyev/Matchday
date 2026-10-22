namespace Matchday.Api.Dtos;

// GET /me/join-requests: a team I asked to join
public record MyJoinRequestDto(int TeamId, string TeamName, string TeamCity, string TeamColor, DateTime CreatedAt);
