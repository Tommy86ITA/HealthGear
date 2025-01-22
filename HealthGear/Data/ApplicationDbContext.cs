using HealthGear.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices { get; set; }

    public DbSet<Maintenance> Maintenances { get; set; }
    public DbSet<MaintenanceSettings> MaintenanceSettings { get; set; }

    public DbSet<MaintenanceDocument> MaintenanceDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Impostazione unicità del numero di serie
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.SerialNumber)
            .IsUnique();
    }
}