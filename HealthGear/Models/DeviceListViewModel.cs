namespace HealthGear.Models;

public class DeviceListViewModel
{
    public List<Device> ActiveDevices { get; set; } = [];
    public List<Device> ArchivedDevices { get; set; } = [];
    public string StatusFilter { get; set; } = "attivi";
}