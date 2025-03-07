using X.PagedList;

// Se usi IPagedList, altrimenti rimuovi se non serve
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace HealthGear.Models.ViewModels;

public class HomeViewModel
{
    public int NumberOfDevices { get; set; }
    public int NumberOfInterventions { get; set; }

    // Ultimi interventi come lista (usata per il recupero)
    public List<Intervention> RecentInterventions { get; set; } = [];

    // Dispositivi con scadenze imminenti
    public List<Device> UpcomingDueDates { get; set; } = [];

    // Proprietà calcolata per ottenere un IPagedList basato su RecentInterventions
    public IPagedList<Intervention> InterventionHistoryModel
        => new StaticPagedList<Intervention>(RecentInterventions, 1, RecentInterventions.Count,
            RecentInterventions.Count);
}