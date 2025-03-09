namespace HealthGear.Models.ViewModels;

public class StatisticsViewModel
{
    public StatisticsViewModel(List<Intervention> interventions)
    {
        TotalInterventions = interventions.Count;
        InterventionsByType = interventions
            .GroupBy(i => i.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Separiamo le manutenzioni preventive e correttive
        PreventiveMaintenances = interventions
            .Where(i => i.Type == InterventionType.Maintenance && i.MaintenanceCategory == MaintenanceType.Preventive)
            .GroupBy(i => "Manutenzione - Preventiva")
            .ToDictionary(g => g.Key, g => g.Count());

        CorrectiveMaintenances = interventions
            .Where(i => i.Type == InterventionType.Maintenance && i.MaintenanceCategory == MaintenanceType.Corrective)
            .GroupBy(i => "Manutenzione - Correttiva")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    ///     Numero totale di interventi registrati
    /// </summary>
    public int TotalInterventions { get; set; }

    /// <summary>
    ///     Distribuzione degli interventi per tipologia (es. Manutenzioni, Verifiche Elettriche, ecc.)
    /// </summary>
    public Dictionary<string, int> InterventionsByType { get; set; }

    // Nuove proprietà per gestire meglio i tipi di intervento
    public Dictionary<string, int> PreventiveMaintenances { get; set; }
    public Dictionary<string, int> CorrectiveMaintenances { get; set; }

    /// <summary>
    ///     Lista dei dispositivi con il maggior numero di interventi
    /// </summary>
    public List<DeviceInterventionCount> TopDevicesByInterventions { get; set; } = new();

    /// <summary>
    ///     Lista degli ultimi interventi registrati
    /// </summary>
    public List<InterventionSummary> RecentInterventions { get; set; } = new();

    /// <summary>
    ///     Numero totale di manutenzioni correttive registrate
    /// </summary>
    public int CorrectiveMaintenanceCount { get; set; }

    /// <summary>
    ///     Lista dei dispositivi con il maggior numero di manutenzioni correttive
    /// </summary>
    public List<DeviceMaintenanceStats> TopDevicesWithCorrectiveMaintenance { get; set; } = [];

    /// <summary>
    ///     Tempo medio (in giorni) tra due manutenzioni correttive
    /// </summary>
    public double AverageTimeBetweenCorrectiveMaintenances { get; set; }
}

/// <summary>
///     Modello per rappresentare il numero di interventi per dispositivo
/// </summary>
public class DeviceInterventionCount
{
    public string DeviceName { get; set; } = string.Empty;
    public int InterventionCount { get; set; }
}

/// <summary>
///     Modello per rappresentare il numero di manutenzioni correttive per dispositivo
/// </summary>
public class DeviceMaintenanceStats
{
    public string DeviceName { get; set; } = string.Empty;
    public int CorrectiveMaintenanceCount { get; set; }
}

/// <summary>
///     Modello per riassumere gli ultimi interventi effettuati
/// </summary>
public class InterventionSummary
{
    public DateTime Date { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}