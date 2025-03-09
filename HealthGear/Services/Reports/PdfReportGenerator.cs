using HealthGear.Models;
using HealthGear.Models.ReportTemplates;

namespace HealthGear.Services.Reports;

public class PdfReportGenerator
{
    public static Task<byte[]> GenerateDeviceListReportAsync(List<Device> devices, string statusFilter)
    {
        return Task.FromResult(DeviceListReportPdf.Generate(devices, statusFilter));
    }
}