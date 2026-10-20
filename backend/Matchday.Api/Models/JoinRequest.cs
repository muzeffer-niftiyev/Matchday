namespace Matchday.Api.Models;

public class JoinRequest
{
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
}
