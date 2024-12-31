using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        try
        {
            // Ensure that the database is created, or apply migrations if using EF migrations
            await context.Database.MigrateAsync();

            // Check if any users exist to avoid seeding again
            if (!context.Users.Any())
            {
                await SeedUsersAsync(context);
            }

            // Check if any threat logs exist to avoid seeding again
            if (!context.ThreatLogs.Any())
            {
                await SeedThreatLogsAsync(context);
            }
        }
        catch (Exception ex)
        {
            // Log the exception (you can integrate a logging framework here)
            Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
            // Optionally, throw or handle the exception based on your needs
        }
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        // Add initial users if none exist
        var users = new[]
        {
            new User
            {
                Username = "admin",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "user1",
                Email = "user1@example.com",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedThreatLogsAsync(ApplicationDbContext context)
    {
        // Add initial threat logs if none exist
        var threatLogs = new[]
        {
            new ThreatLog
            {
                Details = "SQL Injection attempt detected.",
                DetectedAt = DateTime.UtcNow,
                UserId = 1  // Assuming user with ID 1 exists
            },
            new ThreatLog
            {
                Details = "Cross-Site Scripting (XSS) attack detected.",
                DetectedAt = DateTime.UtcNow,
                UserId = 2  // Assuming user with ID 2 exists
            }
        };

        await context.ThreatLogs.AddRangeAsync(threatLogs);
        await context.SaveChangesAsync();
    }
}
