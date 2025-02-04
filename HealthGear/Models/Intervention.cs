#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace HealthGear.Models;

public enum InterventionType
{
    [Display(Name = "Manutenzione")] Maintenance,

    [Display(Name = "Verifica elettrica")] ElectricalTest,

    [Display(Name = "Controlli Fisica Sanitaria")]
    PhysicalInspection
}

public enum MaintenanceType
{
    [Display(Name = "Preventiva")] Preventive,

    [Display(Name = "Correttiva")] Corrective
}

public class Intervention
{
    // Costruttore di default per garantire che le liste siano inizializzate
    public Intervention()
    {
        Attachments = new List<FileDocument>();
    }

    [Key] public int Id { get; set; }

    [Required] public int DeviceId { get; set; }

    [ForeignKey("DeviceId")] public Device? Device { get; set; }

    [Required] public InterventionType Type { get; set; }

    [Required] public DateTime Date { get; set; }

    // Solo per verifiche elettriche e controlli fisici
    public bool? Passed { get; set; }

    [Required] [MaxLength(50)] public string PerformedBy { get; set; } = string.Empty; // Assicuriamoci che non sia null

    // Solo per manutenzioni
    public MaintenanceType? MaintenanceCategory { get; set; }

    public string Notes { get; set; } = string.Empty;

    public List<FileDocument> Attachments { get; set; } = new();
}