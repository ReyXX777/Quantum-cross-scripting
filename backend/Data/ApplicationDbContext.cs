// Quantum-cross-scripting/backend/Data/ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;
using System;

namespace QuantumCrossScripting.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor to pass DbContextOptions to the base class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties to represent tables for User and ThreatLog entities
        public DbSet<User> Users { get; set; }
        public DbSet<ThreatLog> ThreatLogs { get; set; }

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
            });

            // Configurations for ThreatLog entity
            modelBuilder.Entity<ThreatLog>(entity =>
            {
                // Define the primary key
                entity.HasKey(t => t.ThreatLogId);

                // Define required properties
                entity.Property(t => t.Details)
                      .IsRequired();

                entity.Property(t => t.DetectedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Optional foreign key to associate ThreatLogs with Users
                entity.HasOne(t => t.User)
                      .WithMany(u => u.ThreatLogs)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete associated ThreatLogs when User is deleted
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

        // Navigation property to access associated ThreatLogs
        public ICollection<ThreatLog> ThreatLogs { get; set; }
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
}
