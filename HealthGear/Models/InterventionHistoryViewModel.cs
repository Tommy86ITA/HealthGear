using X.PagedList;

namespace HealthGear.Models;

public class InterventionHistoryViewModel
{
    // L'ID del dispositivo di cui mostrare lo storico
    public int DeviceId { get; set; }

    // La lista paginata degli interventi
    public IPagedList<Intervention> Interventions { get; set; } = new StaticPagedList<Intervention>(new List<Intervention>(), 1, 10, 0);

    // Puoi eventualmente aggiungere altre proprietà per filtri o dati extra
}