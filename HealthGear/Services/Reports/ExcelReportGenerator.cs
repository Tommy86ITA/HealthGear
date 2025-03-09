using HealthGear.Models;
using HealthGear.Models.ReportTemplates;

namespace HealthGear.Services.Reports;

public class ExcelReportGenerator(DeadlineService deadlineService)
{
    public Task<byte[]> GenerateDeviceListReportAsync(List<Device> devices)
    {
        return Task.FromResult(DeviceListReportExcel.Generate(devices, deadlineService));
    }
}