namespace Matchday.Api.Dtos;

public record TeamMemberDto(
    int UserId,
    string DisplayName,
    string Role,
    DateTime JoinedAt
);