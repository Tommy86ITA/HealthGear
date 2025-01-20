using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models;

public class Device
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome del dispositivo è obbligatorio.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "La marca è obbligatoria.")]
    [MinLength(2, ErrorMessage = "La marca deve contenere almeno 2 caratteri.")]
    public required string Brand { get; set; }

    [Required(ErrorMessage = "Il modello è obbligatorio.")]
    public required string Model { get; set; }

    [Required(ErrorMessage = "Il numero di serie è obbligatorio.")]
    public required string SerialNumber { get; set; }

    [Required(ErrorMessage = "L'ubicazione è obbligatoria.")]
    [MinLength(5, ErrorMessage = "L'ubicazione deve contenere almeno 5 caratteri.")]
    public required string Location { get; set; }

    public bool CertificationCE { get; set; }
    public bool ManualAvailable { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? MaintenanceDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? NextMaintenance { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ElectricalTestDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? NextElectricalTest { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PhysicalInspectionDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? NextPhysicalInspection { get; set; }

    public string? Notes { get; set; }

    [Required(ErrorMessage = "La categoria è obbligatoria.")]
    public required string Category { get; set; } = "Generico";

    public void CalcolaProssimeDate()
    {
        if (!MaintenanceDate.HasValue)
            return;

        switch (Category?.ToLower())
        {
            case "radiogeno" when Name.ToLower() == "mammografo":
                NextPhysicalInspection = MaintenanceDate.Value.AddMonths(6);
                break;
            case "radiogeno":
                NextPhysicalInspection = MaintenanceDate.Value.AddYears(1);
                break;
            default:
                NextPhysicalInspection = MaintenanceDate.Value.AddYears(1);
                break;
        }

        NextMaintenance = MaintenanceDate.Value.AddYears(1);
        NextElectricalTest = MaintenanceDate.Value.AddYears(2);
    }

    public static ValidationResult? ValidateMaintenanceDate(DateTime? date, ValidationContext context)
    {
        if (date.HasValue && date.Value < DateTime.Today)
            return new ValidationResult("La data di manutenzione non può essere nel passato.");
        return ValidationResult.Success;
    }
}