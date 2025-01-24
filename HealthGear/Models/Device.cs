using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class Device
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Il nome del dispositivo è obbligatorio.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Il produttore è obbligatorio.")]
    public required string Brand { get; set; }

    [Required(ErrorMessage = "Il modello è obbligatorio.")]
    public required string Model { get; set; }

    [Required(ErrorMessage = "Il numero di serie è obbligatorio.")]
    public required string SerialNumber { get; set; }

    public string? Location { get; set; } // Facoltativa

    [Required(ErrorMessage = "La tipologia del dispositivo è obbligatoria.")]
    public required string DeviceType { get; set; } // Radiogeno, Ecografico, Cardiologico, Altro

    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data di collaudo è obbligatoria.")]
    public required DateTime DataCollaudo { get; set; }

    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data della prima verifica elettrica è obbligatoria.")]
    public required DateTime FirstElectricalTest { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FirstPhysicalInspection { get; set; } // Solo per apparecchiature radiogene

    public string? Notes { get; set; }

    // Proprietà deprecate per mantenere compatibilità con le vecchie view
    [NotMapped] public bool CertificationCE { get; set; } = false;

    [NotMapped] public bool ManualAvailable { get; set; } = false;

    [NotMapped] public DateTime? MaintenanceDate => DataCollaudo;

    // Calcoli automatici delle scadenze
    [NotMapped]
    public DateTime? NextMaintenance
    {
        get
        {
            if (Maintenances != null && Maintenances.Any(m => m.MaintenanceType == "Ordinaria"))
            {
                var lastOrdinaryMaintenance = Maintenances
                    .Where(m => m.MaintenanceType == "Ordinaria")
                    .Max(m => m.MaintenanceDate);
                return lastOrdinaryMaintenance != null ? lastOrdinaryMaintenance.Value.AddYears(1) : null;
            }

            return DataCollaudo != default ? DataCollaudo.AddYears(1) : null;
        }
    }

    [NotMapped]
    public DateTime? NextElectricalTest => FirstElectricalTest != default
        ? FirstElectricalTest.AddYears(2)
        : null;

    [NotMapped]
    public DateTime? NextPhysicalInspection
    {
        get
        {
            if (DeviceType.ToLower() == "radiogeno" && FirstPhysicalInspection.HasValue)
                return Model.ToLower().Contains("mammografo")
                    ? FirstPhysicalInspection.Value.AddMonths(6)
                    : FirstPhysicalInspection.Value.AddYears(1);
            return null;
        }
    }

    public ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
}