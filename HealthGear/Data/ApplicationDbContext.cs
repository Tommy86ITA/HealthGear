using HealthGear.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<Intervention> Interventions { get; set; }
    public DbSet<MaintenanceSettings> MaintenanceSettings { get; set; }

    // Aggiungi la nuova proprietà per i file allegati
    public DbSet<FileAttachment> FileAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.Device)
            .WithMany(d => d.Interventions)
            .HasForeignKey(i => i.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}