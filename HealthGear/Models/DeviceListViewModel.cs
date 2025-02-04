namespace HealthGear.Models;

public class DeviceListViewModel
{
    public List<Device> ActiveDevices { get; set; } = new();
    public List<Device> ArchivedDevices { get; set; } = new();
    public string StatusFilter { get; set; } = "attivi";
}