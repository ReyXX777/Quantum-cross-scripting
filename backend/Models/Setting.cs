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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; } // Added UserProfile
        public DbSet<SecurityQuestion> SecurityQuestions { get; set; } // Added SecurityQuestion


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... (Existing configurations for Setting, User, Log, ThreatLog, AuditLog)

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(up => up.Id);
                entity.HasOne(up => up.User)
                    .WithOne(u => u.UserProfile)
                    .HasForeignKey<UserProfile>(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SecurityQuestion>(entity =>
            {
                entity.HasKey(sq => sq.Id);
                entity.Property(sq => sq.Question)
                    .IsRequired()
                    .HasMaxLength(200);
            });
        }
    }

    // ... (Existing classes: Setting, User, ThreatLog, Log, AuditLog)

    public class UserProfile // Added UserProfile class
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string SecurityAnswer { get; set; }

        public int SecurityQuestionId { get; set; }
        public SecurityQuestion SecurityQuestion { get; set; } // Navigation property

        public User User { get; set; } // Navigation property
    }

    public class SecurityQuestion // Added SecurityQuestion class
    {
        public int Id { get; set; }
        public string Question { get; set; }
    }
}

