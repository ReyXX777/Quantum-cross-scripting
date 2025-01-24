using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace QuantumCrossScripting.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor to pass DbContextOptions to the base class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties to represent tables for User, ThreatLog, and AuditLog entities
        public DbSet<User> Users { get; set; }
        public DbSet<ThreatLog> ThreatLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Configure model properties and relationships using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurations for User entity
            modelBuilder.Entity<User>(entity =>
            {
                // Define the primary key
                entity.HasKey(u => u.UserId);

                // Define required properties with max length and constraints
                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Add index for Email (optional) for faster lookups
                entity.HasIndex(u => u.Email).IsUnique();

                // Optional: Add a timestamp for concurrency handling
                entity.Property(u => u.RowVersion)
                      .IsRowVersion();
            });

            // Configurations for ThreatLog entity
            modelBuilder.Entity<ThreatLog>(entity =>
            {
                // Define the primary key
                entity.HasKey(t => t.ThreatLogId);

                // Define required properties
                entity.Property(t => t.Details)
                      .IsRequired()
                      .HasMaxLength(500); // Limiting size for better DB storage

                entity.Property(t => t.DetectedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Optional foreign key to associate ThreatLogs with Users
                entity.HasOne(t => t.User)
                      .WithMany(u => u.ThreatLogs)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete associated ThreatLogs when User is deleted
            });

            // Configurations for AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                // Define the primary key
                entity.HasKey(a => a.AuditLogId);

                // Define required properties
                entity.Property(a => a.Action)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.Details)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(a => a.Timestamp)
                      .HasDefaultValueSql("GETDATE()");

                // Optional foreign key to associate AuditLogs with Users
                entity.HasOne(a => a.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete associated AuditLogs when User is deleted
            });
        }
    }

    // User entity class to represent a user
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        // Add a RowVersion property to handle concurrency
        public byte[] RowVersion { get; set; }  // Used for optimistic concurrency control

        // Navigation property to access associated ThreatLogs
        public ICollection<ThreatLog> ThreatLogs { get; set; }

        // Navigation property to access associated AuditLogs
        public ICollection<AuditLog> AuditLogs { get; set; }
    }

    // ThreatLog entity class to represent a threat detection log
    public class ThreatLog
    {
        public int ThreatLogId { get; set; }
        public string Details { get; set; }
        public DateTime DetectedAt { get; set; }

        // Foreign key property to associate the ThreatLog with a User
        public int UserId { get; set; }

        // Navigation property to access the associated User
        public User User { get; set; }
    }

    // AuditLog entity class to represent an audit log
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }

        // Foreign key property to associate the AuditLog with a User
        public int UserId { get; set; }

        // Navigation property to access the associated User
        public User User { get; set; }
    }
}
