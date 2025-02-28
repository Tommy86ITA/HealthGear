using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace HealthGear.Models;

public class FileAttachment
{
    [Key] public int Id { get; set; }

    // Il nome originale del file caricato
    [Required] public string FileName { get; set; } = string.Empty;

    // Il percorso del file salvato sul server (es. "/uploads/xxx.pdf")
    [Required] public string FilePath { get; set; } = string.Empty;

    // Il tipo MIME del file (es. "application/pdf")
    [Required] public string ContentType { get; set; } = string.Empty;

    // Data e ora di caricamento
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    // Indica il tipo di documento: ad esempio "Collaudo", "Certificazione", "Manuale", "Intervento"
    [Required] public string DocumentType { get; set; } = string.Empty;

    // Associa il file a un dispositivo (opzionale)
    public int? DeviceId { get; set; }

    [ForeignKey("DeviceId")] public Device? Device { get; set; }

    // Associa il file a un intervento (opzionale)
    public int? InterventionId { get; set; }

    [ForeignKey("InterventionId")] public Intervention? Intervention { get; set; }
}