namespace Matchday.Api.Dtos;

public record TeamDto(
    int Id,
    string Name,
    string City,
    string Color,
    int OwnerId,
    string OwnerName,
    int MemberCount,
    DateTime CreatedAt
);