namespace HealthGear.Models.ViewModels;

/// <summary>
/// ViewModel per la gestione delle statistiche sugli interventi.
/// </summary>
public class StatisticsViewModel
{
    public StatisticsViewModel(List<Intervention> interventions)
    {
        TotalInterventions = interventions.Count;
        
        // Distribuzione degli interventi per tipologia
        InterventionsByType = interventions
            .GroupBy(i => i.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Separazione tra manutenzioni preventive e correttive
        PreventiveMaintenances = interventions
            .Where(i => i is { Type: InterventionType.Maintenance, MaintenanceCategory: MaintenanceType.Preventive })
            .GroupBy(i => "Manutenzione - Preventiva")
            .ToDictionary(g => g.Key, g => g.Count());

        CorrectiveMaintenances = interventions
            .Where(i => i is { Type: InterventionType.Maintenance, MaintenanceCategory: MaintenanceType.Corrective })
            .GroupBy(i => "Manutenzione - Correttiva")
            .ToDictionary(g => g.Key, g => g.Count());

        // Calcola i dispositivi con il maggior numero di manutenzioni correttive
        DevicesWithMostCorrectiveMaintenances = interventions
            .Where(i => i.Type == InterventionType.Maintenance && i is { MaintenanceCategory: MaintenanceType.Corrective, Device: not null })
            .GroupBy(i => i.Device!) // Raggruppiamo direttamente per l'oggetto `Device`
            .Select(g => new DeviceCorrectiveMaintenanceStats
            {
                DeviceId = g.Key.Id,
                DeviceName = g.Key.Name,
                DeviceBrand = g.Key.Brand,
                DeviceModel = g.Key.Model,
                CorrectiveMaintenanceCount = g.Count()
            })
            .OrderByDescending(d => d.CorrectiveMaintenanceCount)
            .Take(10)
            .ToList();

        // Calcola il tempo medio tra due manutenzioni correttive per dispositivo
        AverageTimeBetweenCorrectiveMaintenances = CalculateAverageTimeBetweenCorrectiveMaintenances(interventions);

        // Aggiunge gli ultimi interventi per la visualizzazione nella dashboard
        RecentInterventions = interventions
            .OrderByDescending(i => i.Date)
            .Take(10)
            .Select(i => new InterventionSummary
            {
                Date = i.Date,
                DeviceName = i.Device?.Name ?? "Sconosciuto",
                Type = i.Type.ToString(),
                Status = i.Passed.HasValue
                    ? i.Passed.Value ? "Superato" : "Non Superato"
                    : "N/D"
            })
            .ToList();
    }

    /// <summary>
    /// Numero totale di interventi registrati.
    /// </summary>
    public int TotalInterventions { get; set; }

    /// <summary>
    /// Distribuzione degli interventi per tipologia.
    /// </summary>
    public Dictionary<string, int> InterventionsByType { get; set; }

    /// <summary>
    /// Numero di manutenzioni preventive.
    /// </summary>
    public Dictionary<string, int> PreventiveMaintenances { get; set; }

    /// <summary>
    /// Numero di manutenzioni correttive.
    /// </summary>
    public Dictionary<string, int> CorrectiveMaintenances { get; set; }

    /// <summary>
    /// Lista dei dispositivi con il maggior numero di manutenzioni correttive.
    /// </summary>
    public List<DeviceCorrectiveMaintenanceStats> DevicesWithMostCorrectiveMaintenances { get; set; }

    /// <summary>
    /// Tempo medio (in giorni) tra due manutenzioni correttive.
    /// </summary>
    public double AverageTimeBetweenCorrectiveMaintenances { get; set; }

    /// <summary>
    /// Lista degli ultimi interventi registrati.
    /// </summary>
    public List<InterventionSummary> RecentInterventions { get; set; }

    /// <summary>
    /// Calcola il tempo medio (in giorni) tra due manutenzioni correttive.
    /// </summary>
    private static double CalculateAverageTimeBetweenCorrectiveMaintenances(List<Intervention> interventions)
    {
        var correctiveMaintenancesByDevice = interventions
            .Where(i => i.Type == InterventionType.Maintenance && i.MaintenanceCategory == MaintenanceType.Corrective && i.Device != null)
            .GroupBy(i => i.Device!.Id) // Raggruppiamo direttamente per ID per evitare ambiguità
            .ToDictionary(grouping => grouping.Key, grouping => grouping.OrderBy(i => i.Date).Select(i => i.Date).ToList());

        var timeDifferences = new List<double>();

        foreach (var dates in correctiveMaintenancesByDevice.Select(device => device.Value).Where(dates => dates.Count >= 2))
        {
            for (var i = 1; i < dates.Count; i++)
            {
                timeDifferences.Add((dates[i] - dates[i - 1]).TotalDays);
            }
        }

        return timeDifferences.Count != 0 ? timeDifferences.Average() : 0;
    }

    /// <summary>
    /// Rappresenta un riepilogo degli ultimi interventi.
    /// </summary>
    public class InterventionSummary
    {
        public DateTime Date { get; set; }
        public string DeviceName { get; set; } = "Sconosciuto";
        public string Type { get; set; } = "N/D";
        public string Status { get; set; } = "N/D";
    }

    /// <summary>
    /// Rappresenta un dispositivo con il numero di manutenzioni correttive effettuate.
    /// </summary>
    public class DeviceCorrectiveMaintenanceStats
    {
        public string DeviceName { get; set; } = "Sconosciuto";
        public string DeviceBrand { get; set; } = "Sconosciuto";
        public string DeviceModel { get; set; } = "Sconosciuto";
        public int DeviceId { get; set; }
        public int CorrectiveMaintenanceCount { get; set; }
    }
}