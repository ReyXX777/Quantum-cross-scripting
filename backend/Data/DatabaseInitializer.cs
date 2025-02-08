using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly ApplicationDbContext _context; // Inject the DbContext

    public DatabaseInitializer(ILogger<DatabaseInitializer> logger, ApplicationDbContext context) // Add DbContext to constructor
    {
        _logger = logger;
        _context = context; // Assign it to the field
    }

    public async Task InitializeAsync() // No longer takes context as argument
    {
        try
        {
            await _context.Database.MigrateAsync(); // Use the injected context

            if (!_context.Users.Any())
            {
                await SeedUsersAsync();
            }

            if (!_context.ThreatLogs.Any())
            {
                await SeedThreatLogsAsync();
            }

            if (!_context.AuditLogs.Any())
            {
                await SeedAuditLogsAsync();
            }

            if (!_context.Settings.Any())
            {
                await SeedSettingsAsync();
            }

            if (!_context.Roles.Any())
            {
                await SeedRolesAsync();
            }

            if (!_context.UserRoles.Any())
            {
                await SeedUserRolesAsync();
            }

            if (!_context.SecurityQuestions.Any())
            {
                await SeedSecurityQuestionsAsync(); // Seed Security Questions
            }

            if (!_context.UserProfiles.Any()) // Seed User Profiles
            {
                await SeedUserProfilesAsync();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    // ... (Seed methods remain mostly the same, but use _context)

    private async Task SeedUsersAsync()
    {
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

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Users seeded successfully.");
    }

    // ... (Other Seed methods - ThreatLogsAsync, AuditLogsAsync, SettingsAsync, RolesAsync, UserRolesAsync - use _context similarly)

    private async Task SeedSecurityQuestionsAsync()
    {
        var securityQuestions = new[]
        {
            new SecurityQuestion { Question = "What is your mother's maiden name?" },
            new SecurityQuestion { Question = "What was your first pet's name?" },
            new SecurityQuestion { Question = "What city were you born in?" }
        };

        await _context.SecurityQuestions.AddRangeAsync(securityQuestions);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Security questions seeded successfully.");
    }

    private async Task SeedUserProfilesAsync()
    {
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var user1 = await _context.Users.FirstOrDefaultAsync(u => u.Username == "user1");

        if (adminUser == null || user1 == null)
        {
            _logger.LogError("Users not found for user profiles seeding.");
            return;
        }


        var userProfiles = new[]
        {
            new UserProfile { UserId = adminUser.UserId, FirstName = "Admin", LastName = "User" },
            new UserProfile { UserId = user1.UserId, FirstName = "Test", LastName = "User" }
        };

        await _context.UserProfiles.AddRangeAsync(userProfiles);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User profiles seeded successfully.");
    }



}

