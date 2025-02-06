#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace HealthGear.Models;

public class MaintenanceSettings
{
    [Key] public int Id { get; init; }

    [Required] public int MaintenanceIntervalMonths { get; set; } = 12;

    [Required] public int ElectricalTestIntervalMonths { get; set; } = 24;

    [Required] public int PhysicalInspectionIntervalMonths { get; set; } = 12;

    [Required] public int MammographyInspectionIntervalMonths { get; set; } = 6;

    // Campi calcolati in anni
    [NotMapped] public int ElectricalTestIntervalYears => ElectricalTestIntervalMonths / 12;

    [NotMapped] public int PhysicalInspectionIntervalYears => PhysicalInspectionIntervalMonths / 12;
}