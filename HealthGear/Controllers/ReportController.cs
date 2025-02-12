using HealthGear.Data;
using HealthGear.Services.Reports;
using HealthGear.Services.Reports.ReportTemplates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

[Route("Report")]
public class ReportController(
    ApplicationDbContext context,
    PdfReportGenerator pdfReportGenerator,
    ExcelReportGenerator excelReportGenerator)
    : Controller
{
    private readonly ApplicationDbContext _context = context;

    // 📌 Genera il report PDF (tutti i dispositivi)
    [HttpGet("GenerateDeviceListPdf")]
    public async Task<IActionResult> GenerateDeviceListPdf()
    {
        var pdfBytes = await pdfReportGenerator.GenerateDeviceListReportAsync();
        return File(pdfBytes, "application/pdf", "Report_Dispositivi.pdf");
    }

    // 📌 Genera il report Excel (tutti i dispositivi)
    [HttpGet("GenerateDeviceListExcel")]
    public async Task<IActionResult> GenerateDeviceListExcel()
    {
        var excelBytes = await excelReportGenerator.GenerateDeviceListReportAsync();
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Report_Dispositivi.xlsx");
    }

    // 📌 Genera il report PDF per un singolo dispositivo
    [HttpGet("GenerateDeviceDetailPdf/{id:int}")]
    public async Task<IActionResult> GenerateDeviceDetailPdf(int id)
    {
        var device = await _context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();

        var pdfBytes = DeviceDetailsReport.Generate(device);
        return File(pdfBytes, "application/pdf", $"Report_{device.Name}.pdf");
    }

    // 📌 Genera il report Excel per un singolo dispositivo
    [HttpGet("GenerateDeviceDetailExcel/{id:int}")]
    public async Task<IActionResult> GenerateDeviceDetailExcel(int id)
    {
        var device = await _context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();

        var fileContents = DeviceDetailsExcel.Generate(device);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Report_{device.Name}.xlsx");
    }
}