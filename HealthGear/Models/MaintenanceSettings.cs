using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models;

public class MaintenanceSettings
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "La periodicità delle manutenzioni è obbligatoria.")]
    [Range(1, 60, ErrorMessage = "Il valore deve essere tra 1 e 60 mesi.")]
    public int MaintenanceIntervalMonths { get; set; } = 12;

    [Required(ErrorMessage = "La periodicità delle verifiche elettriche è obbligatoria.")]
    [Range(1, 60, ErrorMessage = "Il valore deve essere tra 1 e 60 mesi.")]
    public int ElectricalTestIntervalMonths { get; set; } = 24;

    [Required(ErrorMessage = "La periodicità dei controlli fisici è obbligatoria.")]
    [Range(1, 60, ErrorMessage = "Il valore deve essere tra 1 e 60 mesi.")]
    public int PhysicalInspectionIntervalMonths { get; set; } = 12;

    [Required(ErrorMessage = "La periodicità del mammografo è obbligatoria.")]
    [Range(1, 60, ErrorMessage = "Il valore deve essere tra 1 e 60 mesi.")]
    public int MammographyInspectionIntervalMonths { get; set; } = 6;
}