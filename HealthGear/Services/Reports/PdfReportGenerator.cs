using HealthGear.Data;
using HealthGear.Services.Reports.ReportTemplates;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Services.Reports;

public class PdfReportGenerator(ApplicationDbContext context)
{
    public async Task<byte[]> GenerateDeviceListReportAsync()
    {
        var devices = await context.Devices.Include(d => d.Interventions).ToListAsync();
        return DeviceListReport.Generate(devices);
    }
}