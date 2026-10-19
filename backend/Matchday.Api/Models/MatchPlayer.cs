namespace Matchday.Api.Models;

public class MatchPlayer
{
    public int MatchId { set; get; }
    public Match? Match { set; get; }
    public int UserId { set; get; }
    public User? User { set; get; }
    public int TeamId { set; get; }
    public Team? Team { set; get; }
}