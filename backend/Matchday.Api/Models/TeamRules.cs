namespace Matchday.Api.Models;

public static class TeamRules
{
    public const int PlayersPerMatch = 8;
    public const int MaxSquadSize = 25;
    public static readonly TimeSpan InvitationExpiry = TimeSpan.FromHours(5);
}