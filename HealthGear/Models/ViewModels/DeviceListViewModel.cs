using X.PagedList;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace HealthGear.Models.ViewModels;

public class DeviceListViewModel
{
    public IPagedList<Device> ActiveDevices { get; set; } = new StaticPagedList<Device>(new List<Device>(), 1, 10, 0);
    public IPagedList<Device> ArchivedDevices { get; set; } = new StaticPagedList<Device>(new List<Device>(), 1, 10, 0);
    public string StatusFilter { get; set; } = "attivi";
}