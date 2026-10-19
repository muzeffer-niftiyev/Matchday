namespace Matchday.Api.Models;

public class Field
{
    public int Id { set; get; }
    public required string Name { set; get; }
    public required string City { set; get; }
    public GrassType GrassType { set; get; }
    public decimal PricePerHour { set; get; }
}