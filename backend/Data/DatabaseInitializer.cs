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

            // Check if any audit logs exist to avoid seeding again
            if (!context.AuditLogs.Any())
            {
                await SeedAuditLogsAsync(context);
            }

            // Check if any settings exist to avoid seeding again
            if (!context.Settings.Any())
            {
                await SeedSettingsAsync(context);
            }

            // Check if any roles exist to avoid seeding again
            if (!context.Roles.Any())
            {
                await SeedRolesAsync(context);
            }

            // Check if any user roles exist to avoid seeding again
            if (!context.UserRoles.Any())
            {
                await SeedUserRolesAsync(context);
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

    private async Task SeedAuditLogsAsync(ApplicationDbContext context)
    {
        // Ensure the users exist before seeding audit logs
        var user1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var user2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "user1");

        if (user1 == null || user2 == null)
        {
            _logger.LogError("Users not found for audit logs seeding.");
            return;  // Don't proceed with seeding if users are not found
        }

        // Add initial audit logs if none exist
        var auditLogs = new[]
        {
            new AuditLog
            {
                Action = "User Login",
                Details = "User admin logged in successfully.",
                Timestamp = DateTime.UtcNow,
                UserId = user1.UserId
            },
            new AuditLog
            {
                Action = "User Login",
                Details = "User user1 logged in successfully.",
                Timestamp = DateTime.UtcNow,
                UserId = user2.UserId
            }
        };

        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();
        _logger.LogInformation("Audit logs seeded successfully.");
    }

    private async Task SeedSettingsAsync(ApplicationDbContext context)
    {
        // Add initial settings if none exist
        var settings = new[]
        {
            new Settings
            {
                Setting1 = "DefaultSetting1",
                Setting2 = "DefaultSetting2"
            }
        };

        await context.Settings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
        _logger.LogInformation("Settings seeded successfully.");
    }

    private async Task SeedRolesAsync(ApplicationDbContext context)
    {
        // Add initial roles if none exist
        var roles = new[]
        {
            new Role
            {
                RoleName = "Admin"
            },
            new Role
            {
                RoleName = "User"
            }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
        _logger.LogInformation("Roles seeded successfully.");
    }

    private async Task SeedUserRolesAsync(ApplicationDbContext context)
    {
        // Ensure the users and roles exist before seeding user roles
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var user1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "user1");
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");

        if (adminUser == null || user1 == null || adminRole == null || userRole == null)
        {
            _logger.LogError("Users or roles not found for user roles seeding.");
            return;  // Don't proceed with seeding if users or roles are not found
        }

        // Add initial user roles if none exist
        var userRoles = new[]
        {
            new UserRole
            {
                UserId = adminUser.UserId,
                RoleId = adminRole.RoleId
            },
            new UserRole
            {
                UserId = user1.UserId,
                RoleId = userRole.RoleId
            }
        };

        await context.UserRoles.AddRangeAsync(userRoles);
        await context.SaveChangesAsync();
        _logger.LogInformation("User roles seeded successfully.");
    }
}

// Settings entity class to represent application settings
public class Settings
{
    public int SettingsId { get; set; }
    public string Setting1 { get; set; }
    public string Setting2 { get; set; }
}

// Role entity class to represent a role
public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }

    // Navigation property to access associated UserRoles
    public ICollection<UserRole> UserRoles { get; set; }
}

// UserRole entity class to represent a many-to-many relationship between User and Role
public class UserRole
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; }
}
