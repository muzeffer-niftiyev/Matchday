using System.Security.Claims;
using Matchday.Api.Data;
using Matchday.Api.Dtos;
using Matchday.Api.Models;
using Matchday.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Matchday.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (RegisterDto dto, MatchdayContext db, IPasswordHasher<User> hasher) =>
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var emailTaken = await db.Users.AnyAsync(u => u.Email == email);

            if (emailTaken)
            {
                return Results.Conflict("This email is already registered.");
            }

            var user = new User
            {
                Email = email,
                DisplayName = dto.DisplayName.Trim(),
                PasswordHash = "",
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            db.Users.Add(user);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("This email is already registered.");
            }

            return Results.Created($"/users/{user.Id}", new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAt));
        });

        group.MapPost("/login", async (LoginDto dto, MatchdayContext db, IPasswordHasher<User> hasher, TokenService tokens) =>
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Results.Unauthorized();
            }

            var token = tokens.CreateToken(user);

            return Results.Ok(new AuthResponseDto(token));
        });

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                Id = user.FindFirstValue("sub"),
                Email = user.FindFirstValue("email"),
                Name = user.FindFirstValue("name")
            });
        }).RequireAuthorization();
    }
}