namespace Matchday.Api.Dtos;

public record FieldDto
(
    int Id,
    string Name,
    string City,
    string GrassType,
    decimal PricePerHour
);
