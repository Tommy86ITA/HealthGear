using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class MaintenanceDocument
{
    // Costruttore per inizializzare i valori di default
    public MaintenanceDocument()
    {
        UploadedAt = DateTime.UtcNow;
    }

    [Key] public int Id { get; set; }

    [Required] public int MaintenanceId { get; set; }

    [ForeignKey("MaintenanceId")] public Maintenance? Maintenance { get; set; } // Nullable per evitare warning

    [Required] [MaxLength(255)] public required string FileName { get; set; } = string.Empty;

    [Required] [MaxLength(255)] public required string FilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}