using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Models.ReportTemplates;
using HealthGear.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

[Authorize]
[Route("Report")]
public class ReportController(
    ApplicationDbContext context,
    ExcelReportGenerator excelReportGenerator)
    : Controller
{
    // 📌 Genera il report PDF con filtro sui dispositivi
    [HttpGet("GenerateDeviceListPdf")]
    public async Task<IActionResult> GenerateDeviceListPdf(string statusFilter = "all")
    {
        var devices = await GetFilteredDevices(statusFilter);
        var pdfBytes = await PdfReportGenerator.GenerateDeviceListReportAsync(devices, statusFilter);
        return File(pdfBytes, "application/pdf", "Report_Dispositivi.pdf");
    }

// 📌 Genera il report Excel con filtro sui dispositivi
    [HttpGet("GenerateDeviceListExcel")]
    public async Task<IActionResult> GenerateDeviceListExcel(string statusFilter = "all")
    {
        var devices = await GetFilteredDevices(statusFilter);
        var excelBytes = await excelReportGenerator.GenerateDeviceListReportAsync(devices);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Report_Dispositivi.xlsx");
    }

// 📌 Metodo per filtrare i dispositivi in base alla selezione dell'utente
    private async Task<List<Device>> GetFilteredDevices(string statusFilter)
    {
        IQueryable<Device> query = context.Devices;

        query = statusFilter switch
        {
            "attivi" => query.Where(d => d.Status == DeviceStatus.Attivo),
            "dismessi" => query.Where(d => d.Status == DeviceStatus.Dismesso),
            _ => query // "all" restituisce tutti i dispositivi
        };

        return await query.ToListAsync();
    }

    // 📌 Genera il report PDF per un singolo dispositivo
    [HttpGet("GenerateDeviceDetailPdf/{id:int}")]
    public async Task<IActionResult> GenerateDeviceDetailPdf(int id)
    {
        var device = await context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();

        var pdfBytes = DeviceDetailsReportPdf.Generate(device);
        return File(pdfBytes, "application/pdf", $"Report_{device.Name}.pdf");
    }

    // 📌 Genera il report Excel per un singolo dispositivo
    [HttpGet("GenerateDeviceDetailExcel/{id:int}")]
    public async Task<IActionResult> GenerateDeviceDetailExcel(int id)
    {
        var device = await context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();

        var fileContents = DeviceDetailsReportExcel.Generate(device);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Report_{device.Name}.xlsx");
    }

    // 📌 Genera il report riepilogativo in PDF
    [HttpGet("GenerateSummaryReportPdf")]
    public async Task<IActionResult> GenerateSummaryReportPdf()
    {
        var devices = await context.Devices
            .Include(d => d.Interventions)
            .ToListAsync();

        if (devices.Count == 0)
            return NotFound("Nessun dispositivo trovato per generare il report.");

        var pdfBytes = SummaryReportPdf.Generate(devices);
        return File(pdfBytes, "application/pdf", "Report_Riepilogativo.pdf");
    }
}