using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(ILogger<DatabaseInitializer> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync(ApplicationDbContext context)
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
            _logger.LogError(ex, "An error occurred while initializing the database.");
            // Optionally, throw or handle the exception based on your needs
            throw;
        }
    }

    private async Task SeedUsersAsync(ApplicationDbContext context)
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
        _logger.LogInformation("Users seeded successfully.");
    }

    private async Task SeedThreatLogsAsync(ApplicationDbContext context)
    {
        // Ensure the users exist before seeding threat logs
        var user1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var user2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "user1");

        if (user1 == null || user2 == null)
        {
            _logger.LogError("Users not found for threat logs seeding.");
            return;  // Don't proceed with seeding if users are not found
        }

        // Add initial threat logs if none exist
        var threatLogs = new[]
        {
            new ThreatLog
            {
                Details = "SQL Injection attempt detected.",
                DetectedAt = DateTime.UtcNow,
                UserId = user1.UserId  // Use the dynamically retrieved user IDs
            },
            new ThreatLog
            {
                Details = "Cross-Site Scripting (XSS) attack detected.",
                DetectedAt = DateTime.UtcNow,
                UserId = user2.UserId  // Use the dynamically retrieved user IDs
            }
        };

        await context.ThreatLogs.AddRangeAsync(threatLogs);
        await context.SaveChangesAsync();
        _logger.LogInformation("Threat logs seeded successfully.");
    }
}
