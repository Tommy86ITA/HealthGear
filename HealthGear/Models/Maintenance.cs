using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace HealthGear.Models;

public class Maintenance
{
    [Key] public int Id { get; set; }

    [Required] public int DeviceId { get; set; }

    [ForeignKey("DeviceId")] public Device? Device { get; set; }

    [Display(Name = "Data di Manutenzione")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data della manutenzione è obbligatoria.")]
    [CustomValidation(typeof(Maintenance), nameof(ValidateMaintenanceDate))]
    public DateTime? MaintenanceDate { get; set; }


    [Required(ErrorMessage = "La descrizione della manutenzione è obbligatoria.")]
    public required string Description { get; set; }

    public string? Notes { get; set; }

    [Required(ErrorMessage = "Il tecnico responsabile è obbligatorio.")]
    [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    public required string PerformedBy { get; set; }

    [Required(ErrorMessage = "Il tipo di manutenzione è obbligatorio.")]
    [MaxLength(50, ErrorMessage = "Il tipo di manutenzione non può superare i 50 caratteri.")]
    public required string MaintenanceType { get; set; } // Es. Ordinaria, Straordinaria

    // Relazione con i documenti allegati alla manutenzione
    public ICollection<MaintenanceDocument> Documents { get; set; } = new List<MaintenanceDocument>();

    public static ValidationResult? ValidateMaintenanceDate(string date, ValidationContext context)
    {
        if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var parsedDate))
            if (parsedDate > DateTime.Today)
                return new ValidationResult("Non è possibile selezionare una data futura.");

        return ValidationResult.Success;
    }
}