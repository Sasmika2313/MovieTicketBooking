using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Constants;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        await SeedUserAsync(
            context,
            passwordHasher,
            configuration,
            "Admin",
            UserRoles.Admin);

        await SeedUserAsync(
            context,
            passwordHasher,
            configuration,
            "TheatreManager",
            UserRoles.TheatreManager);
    }

    private static async Task SeedUserAsync(
        ApplicationDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        string roleKey,
        string role)
    {
        var fullName = configuration[$"SeedUsers:{roleKey}:FullName"];
        var email = configuration[$"SeedUsers:{roleKey}:Email"];
        var password = configuration[$"SeedUsers:{roleKey}:Password"];

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Missing development seed configuration for {roleKey}.");
        }

        email = email.Trim().ToLowerInvariant();

        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            return;
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            password);

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }
}