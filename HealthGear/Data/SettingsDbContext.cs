using HealthGear.Models.Config;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Data;

public class SettingsDbContext : DbContext
{
    public SettingsDbContext(DbContextOptions<SettingsDbContext> options) : base(options)
    {
    }

    public DbSet<AppConfig> Configurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configura AppConfig come entità principale
        modelBuilder.Entity<AppConfig>().HasKey(c => c.Id);

        // Configura i tipi posseduti (Owned Types)
        modelBuilder.Entity<AppConfig>().OwnsOne(c => c.Smtp);
        modelBuilder.Entity<AppConfig>().OwnsOne(c => c.Logging);
    }
}