using Microsoft.EntityFrameworkCore;
using System;

namespace QuantumCrossScripting.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }
        // Add other DbSets for your entities
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<ThreatLog> ThreatLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Override OnModelCreating to configure entity relationships, constraints, and other settings
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Setting entity (ensure unique constraint on Key)
            modelBuilder.Entity<Setting>()
                .HasIndex(s => s.Key)  // Make sure the Key is unique for lookup
                .IsUnique();

            // Configure other entities as necessary
            // For example, configure relationships, table names, etc.
            modelBuilder.Entity<Log>()
                .Property(l => l.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");  // Set default value for Timestamp

            modelBuilder.Entity<ThreatLog>()
                .HasOne(tl => tl.User)  // Example: If a ThreatLog has a User
                .WithMany(u => u.ThreatLogs)  // A user can have many threat logs
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Specify the delete behavior

            // Additional model configuration for other entities can go here
        }
    }

    // Example of the Setting entity
    public class Setting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; }  // Unique key for settings

        [MaxLength(500)]
        public string Value { get; set; }

        public DateTime CreatedAt { get; set; }

        // You can add more properties related to settings if needed
    }

    // Example of the User entity
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        // Navigation property for ThreatLogs
        public ICollection<ThreatLog> ThreatLogs { get; set; }  // User can have many ThreatLogs
    }

    // Example of the ThreatLog entity
    public class ThreatLog
    {
        public int Id { get; set; }

        [Required]
        public string Details { get; set; }

        public DateTime DetectedAt { get; set; }

        public int UserId { get; set; }  // Foreign key to User
        public User User { get; set; }  // Navigation property
    }

    // Example of the Log entity
    public class Log
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        // Optional: You can add more properties if needed, such as log level or type
        [MaxLength(50)]
        public string LogLevel { get; set; }  // e.g., "Info", "Warning", "Error"

        [MaxLength(100)]
        public string Source { get; set; }  // The source of the log, such as "AuthController" or "DetectionService"
        
        // Optional: User who generated the log (if applicable)
        public string User { get; set; }  // Assumes you want to track the user, could be null

        public Log()
        {
            Timestamp = DateTime.UtcNow;  // Automatically set the timestamp when the log is created
        }
    }
}
