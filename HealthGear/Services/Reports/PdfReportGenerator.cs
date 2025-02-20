using HealthGear.Models;
using HealthGear.Services.Reports.ReportTemplates;

namespace HealthGear.Services.Reports;

public class PdfReportGenerator
{
    public static Task<byte[]> GenerateDeviceListReportAsync(List<Device> devices, string statusFilter)
    {
        return Task.FromResult(DeviceListReport.Generate(devices, statusFilter)); 
    }
}