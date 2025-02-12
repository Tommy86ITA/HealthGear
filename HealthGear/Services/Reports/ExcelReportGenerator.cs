using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Services.Reports.ReportTemplates;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Services.Reports;

public class ExcelReportGenerator(ApplicationDbContext context, DeadlineService deadlineService)
{
    private readonly ApplicationDbContext _context = context;
    private readonly DeadlineService _deadlineService = deadlineService;

    public async Task<byte[]> GenerateDeviceListReportAsync()
    {
        var devices = await _context.Devices.Include(d => d.Interventions).ToListAsync();
        return DeviceListExcel.Generate(devices, _deadlineService);
    }

    public byte[] GenerateDeviceDetailReport(Device device)
    {
        return DeviceDetailsExcel.Generate(device);
    }
}