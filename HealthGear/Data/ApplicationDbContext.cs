using HealthGear.Models;
using HealthGear.Models.Settings;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Data;

/// <summary>
///     ApplicationDbContext: il contesto del database per HealthGear che integra ASP.NET Identity
///     (tramite IdentityDbContext<ApplicationUser />) insieme alle entità personalizzate.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    ///     Costruttore: riceve le opzioni del DbContext e le passa alla base.
    /// </summary>
    /// <param name="options">Opzioni del DbContext</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet per i dispositivi
    public DbSet<Device> Devices { get; set; }

    // DbSet per gli interventi
    public DbSet<Intervention> Interventions { get; set; }

    // DbSet per le impostazioni di manutenzione
    public DbSet<MaintenanceSettings> MaintenanceSettings { get; set; }

    // DbSet per i file allegati
    public DbSet<FileAttachment> FileAttachments { get; set; }

    /// <summary>
    ///     OnModelCreating: configura le relazioni e altre impostazioni del modello.
    /// </summary>
    /// <param name="modelBuilder">Il ModelBuilder da utilizzare</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Chiama la base per configurare le tabelle di Identity (Utenti, Ruoli, ecc.)
        base.OnModelCreating(modelBuilder);

        // Configurazione della relazione tra Intervention e Device:
        // Un intervento ha un Device, e un dispositivo può avere molti interventi.
        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.Device)
            .WithMany(d => d.Interventions)
            .HasForeignKey(i => i.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}