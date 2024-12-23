public class ApplicationDbContext : DbContext
{
    public DbSet<Setting> Settings { get; set; }

    // Other DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Setting entity (e.g., unique constraints, indexes)
        modelBuilder.Entity<Setting>()
            .HasIndex(s => s.Key)  // Make sure the Key is unique for lookup
            .IsUnique();
    }
}
