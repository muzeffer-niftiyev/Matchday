namespace Matchday.Api.Models;

public class TeamMember
{
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public TeamRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}