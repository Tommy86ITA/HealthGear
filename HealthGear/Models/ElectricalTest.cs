using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class ElectricalTest
{
    [Key] public int Id { get; set; }

    [Required] public int DeviceId { get; set; }

    [ForeignKey("DeviceId")] public virtual Device Device { get; set; } = null!;

    [Required] [DataType(DataType.Date)] public DateTime TestDate { get; set; }

    [Required(ErrorMessage = "L'esito della verifica è obbligatorio.")]
    public bool Passed { get; set; } // True = Superato, False = Non Superato

    [Required] [StringLength(255)] public string PerformedBy { get; set; } = null!;

    public string? Notes { get; set; }

    /// <summary>
    ///     Documenti allegati, ora utilizzando il modello unificato FileDocument
    /// </summary>
    public virtual ICollection<FileDocument> Documents { get; set; } = new List<FileDocument>();
}