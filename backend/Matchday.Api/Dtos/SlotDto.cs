namespace Matchday.Api.Dtos;

public record SlotDto(TimeOnly StartTime, TimeOnly EndTime, bool IsFree);