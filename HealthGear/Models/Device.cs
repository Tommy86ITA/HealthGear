#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

#endregion

namespace HealthGear.Models;

/// <summary>
///     Rappresenta un dispositivo elettromedicale all'interno del sistema HealthGear.
///     Contiene dati identificativi, stato, scadenze e storico degli interventi.
/// </summary>
public class Device
{
    // 📌 IDENTIFICATIVI DEL DISPOSITIVO

    /// <summary>Identificativo univoco del dispositivo.</summary>
    [Key]
    public int Id { get; init; }

    /// <summary>Nome del dispositivo.</summary>
    [Required(ErrorMessage = "Il nome del dispositivo è obbligatorio.")]
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>Produttore del dispositivo.</summary>
    [Required(ErrorMessage = "Il produttore è obbligatorio.")]
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Brand { get; set; }

    /// <summary>Modello del dispositivo.</summary>
    [Required(ErrorMessage = "Il modello è obbligatorio.")]
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Model { get; set; }

    /// <summary>Numero di serie del dispositivo.</summary>
    [Required(ErrorMessage = "Il numero di serie è obbligatorio.")]
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string SerialNumber { get; set; }

    /// <summary>Numero di inventario (opzionale).</summary>
    [Required]
    [MaxLength(15)]
    public string InventoryNumber { get; private set; } = string.Empty;

    /// <summary>Ubicazione fisica del dispositivo (opzionale).</summary>
    [MaxLength(100)]
    public string? Location { get; set; }

    /// <summary>Tipologia del dispositivo.</summary>
    [Required(ErrorMessage = "La tipologia del dispositivo è obbligatoria.")]
    public DeviceType DeviceType { get; set; }

    /// <summary>Stato attuale del dispositivo (Attivo, Guasto, etc.).</summary>
    [Required(ErrorMessage = "Lo stato del dispositivo è obbligatorio.")]
    public DeviceStatus Status { get; set; } = DeviceStatus.Attivo; // Default: "Attivo"

    // 📌 DATE DI RIFERIMENTO

    /// <summary>Data di collaudo del dispositivo.</summary>
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data di collaudo è obbligatoria.")]
    public required DateTime DataCollaudo { get; set; }

    /// <summary>
    ///     DATA DI DISMISSIONE
    /// </summary>
    public DateTime? DataDismissione { get; set; }

    /// <summary>Data della prima verifica elettrica obbligatoria.</summary>
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data della prima verifica elettrica è obbligatoria.")]
    public required DateTime FirstElectricalTest { get; set; }

    /// <summary>Data della prima verifica fisica, se applicabile.</summary>
    [DataType(DataType.Date)]
    public DateTime? FirstPhysicalInspection { get; set; }

    // 📌 VERIFICHE E MANUTENZIONI

    /// <summary>
    ///     Determina se il dispositivo richiede una verifica fisica.
    ///     I dispositivi di tipo 'Radiologico' e 'Mammografico' necessitano di questo controllo.
    /// </summary>
    [NotMapped]
    public bool RequiresPhysicalInspection =>
        DeviceType is DeviceType.Radiologico or DeviceType.Mammografico;

    /// <summary>Collezione degli interventi associati al dispositivo.</summary>
    [InverseProperty("Device")]
    public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();

    /// <summary>
    ///     Restituisce la data dell'ultima manutenzione ordinaria, se disponibile.
    /// </summary>
    [NotMapped]
    public DateTime? LastOrdinaryMaintenance =>
        Interventions.Any(i =>
            i is { Type: InterventionType.Maintenance, MaintenanceCategory: MaintenanceType.Preventive })
            ? Interventions
                .Where(i => i is
                    { Type: InterventionType.Maintenance, MaintenanceCategory: MaintenanceType.Preventive })
                .OrderByDescending(i => i.Date)
                .Select(i => i.Date)
                .FirstOrDefault()
            : DataCollaudo;

    /// <summary>
    ///     Restituisce la data dell'ultima verifica elettrica, se disponibile.
    /// </summary>
    [NotMapped]
    public DateTime? LastElectricalTest =>
        Interventions.Any(i => i.Type == InterventionType.ElectricalTest)
            ? Interventions
                .Where(i => i.Type == InterventionType.ElectricalTest)
                .OrderByDescending(i => i.Date)
                .Select(i => i.Date)
                .FirstOrDefault()
            : FirstElectricalTest;

    /// <summary>
    ///     Restituisce la data dell'ultima verifica fisica, se disponibile.
    /// </summary>
    [NotMapped]
    public DateTime? LastPhysicalInspection =>
        Interventions.Any(i => i.Type == InterventionType.PhysicalInspection)
            ? Interventions
                .Where(i => i.Type == InterventionType.PhysicalInspection)
                .OrderByDescending(i => i.Date)
                .Select(i => i.Date)
                .FirstOrDefault()
            : FirstPhysicalInspection;

    // 📌 SCADENZE PROGRAMMATE

    /// <summary>Prossima scadenza della manutenzione preventiva.</summary>
    public DateTime? NextMaintenanceDue { get; set; }

    /// <summary>Prossima scadenza della verifica elettrica.</summary>
    public DateTime? NextElectricalTestDue { get; set; }

    /// <summary>Prossima scadenza della verifica fisica, se applicabile.</summary>
    public DateTime? NextPhysicalInspectionDue { get; set; }

    // 📌 NOTE AGGIUNTIVE

    /// <summary>Note opzionali relative al dispositivo.</summary>
    [MaxLength(2000)]
    public string? Notes { get; set; } = "";

    /// <summary>
    ///     Collezione dei file allegati al dispositivo (ad es. verbale di collaudo, certificazione CE, manuale, ecc.).
    /// </summary>
    public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();

    /// <summary>
    ///     Imposta il numero di inventario solo se non è già stato assegnato.
    /// </summary>
    public void SetInventoryNumber(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(InventoryNumber)) InventoryNumber = inventoryNumber;
    }
}