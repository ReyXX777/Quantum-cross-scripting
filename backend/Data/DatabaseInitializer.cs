public class DatabaseInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Ensure that the database is created
        context.Database.EnsureCreated();

        // Check if any users exist to avoid seeding again
        if (!context.Users.Any())
        {
            SeedUsers(context);
        }

        // Check if any threat logs exist to avoid seeding again
        if (!context.ThreatLogs.Any())
        {
            SeedThreatLogs(context);
        }
    }

    private static void SeedUsers(ApplicationDbContext context)
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

        context.Users.AddRange(users);
        context.SaveChanges();
    }

    private static void SeedThreatLogs(ApplicationDbContext context)
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

        context.ThreatLogs.AddRange(threatLogs);
        context.SaveChanges();
    }
}
