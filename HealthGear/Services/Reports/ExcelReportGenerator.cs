using HealthGear.Models;
using HealthGear.Services.Reports.ReportTemplates;

namespace HealthGear.Services.Reports;

public class ExcelReportGenerator(DeadlineService deadlineService)
{
    public Task<byte[]> GenerateDeviceListReportAsync(List<Device> devices)
    {
        return Task.FromResult(DeviceListExcel.Generate(devices, deadlineService));
    }
}