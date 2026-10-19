using System.Text.RegularExpressions;
using Matchday.Api.Data;
using Matchday.Api.Dtos;
using Matchday.Api.Models;
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
                    f.GrassType.ToString(),
                    f.PricePerHour))
                .ToListAsync();

            return Results.Ok(fields);
        });

        group.MapGet("/{id:int}", async (int id, MatchdayContext db) =>
        {
            var field = await db.Fields.FirstOrDefaultAsync(f => f.Id == id);

            if (field is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new FieldDto(field.Id, field.Name, field.City, field.GrassType.ToString(), field.PricePerHour));
        });

        group.MapPost("/", async (CreateFieldDto dto, MatchdayContext db) =>
        {
            var field = new Field
            {
                Name = dto.Name,
                City = dto.City,
                GrassType = dto.GrassType,
                PricePerHour = dto.PricePerHour
            };

            db.Fields.Add(field);
            await db.SaveChangesAsync();

            return Results.Created($"/fields/{field.Id}", new FieldDto(field.Id, field.Name, field.City, field.GrassType.ToString(), field.PricePerHour));
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:int}", async (int id, CreateFieldDto dto, MatchdayContext db) =>
        {
            var field = await db.Fields.FirstOrDefaultAsync(f => f.Id == id);

            if (field is null)
            {
                return Results.NotFound();
            }

            field.Name = dto.Name;
            field.City = dto.City;
            field.GrassType = dto.GrassType;
            field.PricePerHour = dto.PricePerHour;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        group.MapDelete("/{id:int}", async (int id, MatchdayContext db) =>
        {
            var field = await db.Fields.FirstOrDefaultAsync(f => f.Id == id);

            if (field is null)
            {
                return Results.NotFound();
            }

            db.Fields.Remove(field);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization("Admin");

        group.MapGet("/{id:int}/slots", async (int id, DateOnly date, MatchdayContext db) =>
        {
            var field = await db.Fields.FirstOrDefaultAsync(f => f.Id == id);

            if (field is null)
            {
                return Results.NotFound();
            }

            var now = DateTime.UtcNow;

            var takenTimes = await db.Matches
                .Where(m => m.FieldId == id && m.Date == date)
                .Where(m => m.Status == MatchStatus.Confirmed
                         || (m.Status == MatchStatus.Pending && m.ExpiresAt > now))
                .Select(m => m.StartTime)
                .ToListAsync();

            var slots = new List<SlotDto>();

            foreach (var start in BookingSlots.StartTimes)
            {
                var isFree = !takenTimes.Contains(start);
                slots.Add(new SlotDto(start, start.Add(BookingSlots.Duration), isFree));
            }

            return Results.Ok(slots);
        });
    }
}