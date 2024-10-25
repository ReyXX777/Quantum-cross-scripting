// Quantum-cross-scripting/backend/Data/ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;

namespace QuantumCrossScripting.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties for entities
        public DbSet<User> Users { get; set; }
        public DbSet<ThreatLog> ThreatLogs { get; set; }

        // Configure model properties and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurations for User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configurations for ThreatLog entity
            modelBuilder.Entity<ThreatLog>(entity =>
            {
                entity.HasKey(t => t.ThreatLogId);
                entity.Property(t => t.Details).IsRequired();
                entity.Property(t => t.DetectedAt).HasDefaultValueSql("GETDATE()");

                // Optional foreign key to associate threat logs with a user
                entity.HasOne(t => t.User)
                      .WithMany(u => u.ThreatLogs)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
