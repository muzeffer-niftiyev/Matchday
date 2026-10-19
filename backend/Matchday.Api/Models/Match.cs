namespace Matchday.Api.Models;

public class Match
{
    public int Id { set; get; }
    public int FieldId { set; get; }
    public Field? Field { set; get; }
    public DateOnly Date { set; get; }
    public TimeOnly StartTime { set; get; }
    public int HomeTeamId { set; get; }
    public Team? HomeTeam { set; get; }
    public int AwayTeamId { set; get; }
    public Team? AwayTeam { set; get; }
    public MatchStatus Status { set; get; }
    public DateTime CreatedAt { set; get; }
    public DateTime ExpiresAt { set; get; }
    public int? HomeScore { set; get; }
    public int? AwayScore { set; get; }
}