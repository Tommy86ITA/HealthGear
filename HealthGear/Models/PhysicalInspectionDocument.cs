using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class PhysicalInspectionDocument
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PhysicalInspectionId { get; set; }

    [ForeignKey("PhysicalInspectionId")]
    public PhysicalInspection? PhysicalInspection { get; set; }

    [Required(ErrorMessage = "Il nome del file è obbligatorio.")]
    [MaxLength(255, ErrorMessage = "Il nome del file non può superare i 255 caratteri.")]
    public string FileName { get; set; }

    [Required(ErrorMessage = "Il percorso del file è obbligatorio.")]
    [MaxLength(500, ErrorMessage = "Il percorso del file non può superare i 500 caratteri.")]
    public string FilePath { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}