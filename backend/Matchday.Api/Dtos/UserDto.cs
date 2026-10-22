namespace Matchday.Api.Dtos;

public record UserDto(
    int Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt,
    string Role
);