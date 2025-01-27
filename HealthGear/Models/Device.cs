using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class Device
{
    [Key] 
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome del dispositivo è obbligatorio.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Il produttore è obbligatorio.")]
    public required string Brand { get; set; }

    [Required(ErrorMessage = "Il modello è obbligatorio.")]
    public required string Model { get; set; }

    [Required(ErrorMessage = "Il numero di serie è obbligatorio.")]
    public required string SerialNumber { get; set; }

    // Ubicazione del dispositivo (opzionale)
    public string? Location { get; set; }

    [Required(ErrorMessage = "La tipologia del dispositivo è obbligatoria.")]
    public required string DeviceType { get; set; } // Radiogeno, Ecografico, Cardiologico, Altro

    // Data di collaudo obbligatoria
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data di collaudo è obbligatoria.")]
    public required DateTime DataCollaudo { get; set; }

    // Data della prima verifica elettrica obbligatoria
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data della prima verifica elettrica è obbligatoria.")]
    public required DateTime FirstElectricalTest { get; set; }

    // Data della prima ispezione fisica (solo per apparecchiature radiogene)
    [DataType(DataType.Date)]
    public DateTime? FirstPhysicalInspection { get; set; }

    // Note aggiuntive sul dispositivo
    public string? Notes { get; set; }

    // Proprietà deprecate per mantenere compatibilità con le vecchie view
    [NotMapped] public bool CertificationCE { get; set; } = false;
    [NotMapped] public bool ManualAvailable { get; set; } = false;

    // Lista delle manutenzioni associate al dispositivo
    public ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    /// <summary>
    /// Aggiorna la scadenza della prossima manutenzione ordinaria senza registrarla come eseguita.
    /// </summary>
    /// <param name="ultimaDataManutenzione">Data dell'ultima manutenzione eseguita</param>
    public void AggiornaProssimaManutenzione(DateTime ultimaDataManutenzione)
    {
        // Calcola la prossima scadenza come un anno dopo l'ultima manutenzione registrata
        if (Maintenances.Any(m => m.MaintenanceType == "Ordinaria"))
        {
            var ultimaOrdinaria = Maintenances
                .Where(m => m.MaintenanceType == "Ordinaria")
                .OrderByDescending(m => m.MaintenanceDate)
                .FirstOrDefault();

            if (ultimaOrdinaria != null)
            {
                ultimaOrdinaria.MaintenanceDate = ultimaDataManutenzione.AddYears(1);
            }
        }
        else
        {
            // Se non ci sono manutenzioni precedenti, la prima scadenza sarà basata sul collaudo
            Maintenances.Add(new Maintenance
            {
                MaintenanceDate = ultimaDataManutenzione.AddYears(1),
                MaintenanceType = "Ordinaria",
                Description = "Manutenzione ordinaria programmata",
                PerformedBy = "Sistema"
            });
        }
    }

    /// <summary>
    /// Aggiorna la data della prossima verifica elettrica (ogni 2 anni).
    /// </summary>
    /// <param name="nuovaData">Data della nuova verifica elettrica</param>
    public void AggiornaProssimaVerificaElettrica(DateTime nuovaData)
    {
        FirstElectricalTest = nuovaData;
    }

    /// <summary>
    /// Aggiorna la data del prossimo controllo fisico (gestisce periodicità per mammografi e altri dispositivi radiogeni).
    /// </summary>
    /// <param name="nuovaData">Data della nuova verifica fisica</param>
    public void AggiornaProssimoControlloFisico(DateTime nuovaData)
    {
        if (DeviceType.ToLower() == "radiogeno")
        {
            FirstPhysicalInspection = nuovaData;
        }
    }

    /// <summary>
    /// Calcolo automatico della prossima manutenzione ordinaria
    /// </summary>
    [NotMapped]
    public DateTime? NextMaintenance
    {
        get
        {
            if (Maintenances != null && Maintenances.Any(m => m.MaintenanceType == "Ordinaria"))
            {
                // Trova la data dell'ultima manutenzione ordinaria e aggiunge un anno
                var lastOrdinaryMaintenance = Maintenances
                    .Where(m => m.MaintenanceType == "Ordinaria")
                    .OrderByDescending(m => m.MaintenanceDate)
                    .FirstOrDefault()?.MaintenanceDate.AddYears(1);
                return lastOrdinaryMaintenance?.AddYears(1);
            }

            // Se non ci sono manutenzioni, la prossima manutenzione è un anno dopo il collaudo
            return DataCollaudo.AddYears(1);
        }
    }

    /// <summary>
    /// Calcolo automatico della prossima verifica elettrica (ogni 2 anni dalla prima verifica).
    /// </summary>
    [NotMapped]
    public DateTime? NextElectricalTest => FirstElectricalTest.AddYears(2);

    /// <summary>
    /// Calcolo automatico del prossimo controllo fisico in base alla tipologia del dispositivo.
    /// </summary>
    [NotMapped]
    public DateTime? NextPhysicalInspection
    {
        get
        {
            if (DeviceType.ToLower() == "radiogeno" && FirstPhysicalInspection.HasValue)
            {
                // Se il dispositivo è un mammografo, la scadenza è semestrale, altrimenti annuale
                return Model.ToLower().Contains("mammografo")
                    ? FirstPhysicalInspection.Value.AddMonths(6)
                    : FirstPhysicalInspection.Value.AddYears(1);
            }
            return null;
        }
    }
}