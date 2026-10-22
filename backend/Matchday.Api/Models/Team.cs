namespace Matchday.Api.Models;

public class Team
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OwnerId { get; set; }
    public User? Owner { get; set; }
    public required string Color { get; set; }
}