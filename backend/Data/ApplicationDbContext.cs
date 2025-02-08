using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace QuantumCrossScripting.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ThreatLog> ThreatLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; } // Added UserProfile DbSet
        public DbSet<SecurityQuestion> SecurityQuestions { get; set; } // Added SecurityQuestion DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.RowVersion)
                    .IsRowVersion();
            });

            modelBuilder.Entity<ThreatLog>(entity =>
            {
                entity.HasKey(t => t.ThreatLogId);
                entity.Property(t => t.Details)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(t => t.DetectedAt)
                    .HasDefaultValueSql("GETDATE()");
                entity.HasOne(t => t.User)
                    .WithMany(u => u.ThreatLogs)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.AuditLogId);
                entity.Property(a => a.Action)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(a => a.Details)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(a => a.Timestamp)
                    .HasDefaultValueSql("GETDATE()");
                entity.HasOne(a => a.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(r => r.RoleName).IsUnique();
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserProfile>(entity => // Configure UserProfile
            {
                entity.HasKey(p => p.UserProfileId);
                entity.HasOne(p => p.User)
                      .WithOne(u => u.UserProfile)
                      .HasForeignKey<UserProfile>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SecurityQuestion>(entity => // Configure Security Questions
            {
                entity.HasKey(sq => sq.SecurityQuestionId);
                entity.Property(sq => sq.Question)
                    .IsRequired()
                    .HasMaxLength(200);

            });
        }
    }

    // ... (User, ThreatLog, AuditLog, Role, UserRole remain the same)

    public class UserProfile // Added UserProfile entity
    {
        public int UserProfileId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; } //Nullable
        public string SecurityAnswer { get; set; }
        public int SecurityQuestionId { get; set; }
        public SecurityQuestion SecurityQuestion { get; set; }
        public User User { get; set; }
    }

    public class SecurityQuestion // Added SecurityQuestion entity
    {
        public int SecurityQuestionId { get; set; }
        public string Question { get; set; }
    }
}

