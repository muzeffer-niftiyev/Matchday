namespace Matchday.Api.Services;

public static class BakuTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku");
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);
    public static DateOnly Today => DateOnly.FromDateTime(Now);
}