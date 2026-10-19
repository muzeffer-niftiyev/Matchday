namespace Matchday.Api.Models;

public static class BookingSlots
{
    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(90);

    public static readonly TimeOnly[] StartTimes =
    [
        new(9, 0), new(10, 30), new(12, 0), new(13, 30), new(15, 0),
        new(16, 30), new(18, 0), new(19, 30), new(21, 0)
    ];
}