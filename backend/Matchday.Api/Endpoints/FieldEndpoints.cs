using Matchday.Api.Data;
using Matchday.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Matchday.Api.Endpoints;

public static class FieldEndpoints
{
    public static void MapFieldEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/fields");

        group.MapGet("/", async (MatchdayContext db) =>
        {
            var fields = await db.Fields
                .OrderBy(f => f.Name)
                .Select(f => new FieldDto(
                    f.Id,
                    f.Name,
                    f.City,
                    f.Size.ToString(),
                    f.GrassType.ToString(),
                    f.PricePerHour))
                .ToListAsync();

            return Results.Ok(fields);
        });

        group.MapGet("/{id:int}", async (int id, MatchdayContext db) =>
        {
            var field = await db.Fields.FirstOrDefaultAsync(f => f.Id == id);

            if(field is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new FieldDto(field.Id, field.Name, field.City, field.Size.ToString(), field.GrassType.ToString(), field.PricePerHour));
        });
    }
}