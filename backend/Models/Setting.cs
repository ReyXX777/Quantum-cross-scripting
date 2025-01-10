using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuantumCrossScripting.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<ThreatLog> ThreatLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Setting entity
            modelBuilder.Entity<Setting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            // Configure the User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure the Log entity
            modelBuilder.Entity<Log>()
                .Property(l => l.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure the ThreatLog entity
            modelBuilder.Entity<ThreatLog>()
                .HasOne(tl => tl.User)
                .WithMany(u => u.ThreatLogs)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class Setting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; }

        [MaxLength(500)]
        public string Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class User
    {
        public User()
        {
            ThreatLogs = new HashSet<ThreatLog>();
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        public ICollection<ThreatLog> ThreatLogs { get; set; }
    }

    public class ThreatLog
    {
        public int Id { get; set; }

        [Required]
        public string Details { get; set; }

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }
    }

    public class Log
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        [RegularExpression("Info|Warning|Error", ErrorMessage = "LogLevel must be 'Info', 'Warning', or 'Error'.")]
        public string LogLevel { get; set; }

        [MaxLength(100)]
        public string Source { get; set; }

        public string User { get; set; }
    }
}
