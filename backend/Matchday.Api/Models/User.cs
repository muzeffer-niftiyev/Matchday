namespace Matchday.Api.Models;

public class User
{
    public int Id { set; get; }
    public required string Email { set; get; }
    public required string DisplayName { set; get; }
    public required string PasswordHash { set; get; }
    public DateTime CreatedAt { set; get; }
    public UserRole Role { set; get; }
}