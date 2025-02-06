using HealthGear.Services.Reports;
using Microsoft.AspNetCore.Mvc;
using HealthGear.Data;
using HealthGear.Services.Reports.ReportTemplates;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

public class ReportController(ApplicationDbContext context, PdfReportGenerator pdfReportGenerator, ExcelReportGenerator excelReportGenerator)
    : Controller
{
    private readonly ApplicationDbContext _context = context;

    // 📌 Genera il report PDF (tutti i dispositivi)
    public async Task<IActionResult> GeneratePdf()
    {
        var pdfBytes = await pdfReportGenerator.GenerateDeviceListReportAsync();
        return File(pdfBytes, "application/pdf", "Report_Dispositivi.pdf");
    }

    // 📌 Genera il report Excel (tutti i dispositivi)
    public async Task<IActionResult> GenerateExcel()
    {
        var excelBytes = await excelReportGenerator.GenerateDeviceListReportAsync();
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report_Dispositivi.xlsx");
    }
    
    public async Task<IActionResult> GenerateDeviceDetailPdf(int id)
    {
        var device = await _context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
        {
            return NotFound();
        }

        var pdfBytes = DeviceDetailsReport.Generate(device);
        return File(pdfBytes, "application/pdf", $"Report_{device.Name}.pdf");
    }
    
    public IActionResult GenerateDeviceDetailsExcel(int id)
    {
        var device = _context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefault(d => d.Id == id);

        if (device == null)
        {
            return NotFound();
        }

        var fileContents = DeviceDetailsExcel.Generate(device);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Report_{device.Name}.xlsx");
    }
}


/*using ClosedXML.Excel;
using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HealthGear.Controllers;

public class ReportController : Controller
{
private readonly ApplicationDbContext _context;
private readonly DeadlineService _deadlineService;

// 📌 Costruttore con Dependency Injection
public ReportController(ApplicationDbContext context, DeadlineService deadlineService)
{
    _context = context;
    _deadlineService = deadlineService;
    Settings.License = LicenseType.Community;
}

// 📌 Generazione report in PDF con QuestPDF
public async Task<IActionResult> GeneratePdf()
{
var devices = await _context.Devices.Include(d => d.Interventions).ToListAsync();

var pdf = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(20);
        page.DefaultTextStyle(x => x.FontSize(10));

        // 📌 Intestazione
        page.Header().Column(header =>
        {
            header.Item().AlignCenter().Row(row =>
            {
                row.AutoItem().Width(15).Image("Assets/Icons/clipboard.png");
                row.AutoItem().Text(" Report Dispositivi")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
            });

            header.Item().AlignCenter().Text($"Generato il {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            header.Item().AlignRight().Text("Made with HealthGear v. 1.0");

            header.Item().LineHorizontal(2).LineColor(Colors.Blue.Darken2);
        });

        // 📌 Contenuto del report
        page.Content().Column(content =>
        {
            foreach (var device in devices)
            {
                var lastMaintenance = device.Interventions
                    .Where(i => i.Type == InterventionType.Maintenance)
                    .OrderByDescending(i => i.Date)
                    .FirstOrDefault();

                var lastElectricalTest = device.Interventions
                    .Where(i => i.Type == InterventionType.ElectricalTest)
                    .OrderByDescending(i => i.Date)
                    .FirstOrDefault();

                var lastPhysicalInspection = device.Interventions
                    .Where(i => i.Type == InterventionType.PhysicalInspection)
                    .OrderByDescending(i => i.Date)
                    .FirstOrDefault();

                content.Item().PaddingVertical(15).Column(deviceSection =>
                {
                    deviceSection.Item().Text($"{device.Brand} {device.Model}")
                        .Bold().FontSize(14);

                    deviceSection.Item().Text($"{device.Name} - S/N: {device.SerialNumber}")
                        .Italic().FontSize(10);

                    deviceSection.Item().Height(5);

                    deviceSection.Item().LineHorizontal(1);
                    deviceSection.Item().Height(5);

                    deviceSection.Item().Row(row =>
                    {
                        row.ConstantItem(10).Image("Assets/Icons/calendar-check.png");
                        row.RelativeItem().Text($" Data collaudo: {device.DataCollaudo:dd/MM/yyyy}");
                    });

                    deviceSection.Item().Height(5);

                    deviceSection.Item().Row(row =>
                    {
                        row.ConstantItem(10).Image("Assets/Icons/wrench.png");
                        row.RelativeItem().Text(" Ultima manutenzione:").Bold();
                        row.RelativeItem().Text($"{(lastMaintenance?.Date.ToString("dd/MM/yyyy") ?? "N/A")}");
                        row.ConstantItem(10).Image("Assets/Icons/calendar-x-mark.png");
                        row.RelativeItem().Text(" Scadenza:").Bold();
                        row.RelativeItem().Text($"{(device.NextMaintenanceDue?.ToString("dd/MM/yyyy") ?? "N/A")}");
                    });

                    deviceSection.Item().Row(row =>
                    {
                        row.ConstantItem(10).Image("Assets/Icons/bolt.png");
                        row.RelativeItem().Text(" Ultima verifica elettrica:").Bold();
                        row.RelativeItem().Text($"{(lastElectricalTest?.Date.ToString("dd/MM/yyyy") ?? "N/A")}");
                        row.ConstantItem(10).Image("Assets/Icons/calendar-x-mark.png");
                        row.RelativeItem().Text(" Scadenza:").Bold();
                        row.RelativeItem().Text($"{(device.NextElectricalTestDue?.ToString("dd/MM/yyyy") ?? "N/A")}");
                    });

                    deviceSection.Item().Row(row =>
                    {
                        row.ConstantItem(10).Image("Assets/Icons/radiation.png");
                        row.RelativeItem().Text(" Ultimo controllo fisico:").Bold();
                        row.RelativeItem().Text($"{(lastPhysicalInspection?.Date.ToString("dd/MM/yyyy") ?? "N/A")}");
                        row.ConstantItem(10).Image("Assets/Icons/calendar-x-mark.png");
                        row.RelativeItem().Text(" Scadenza:").Bold();
                        row.RelativeItem().Text($"{(device.NextPhysicalInspectionDue?.ToString("dd/MM/yyyy") ?? "N/A")}");
                    });
                });
            }
        });

        // 📌 Footer con numero di pagina
        page.Footer().AlignCenter()
            .Text(text =>
            {
                text.Span("Pag. ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
    });
});

using var stream = new MemoryStream();
pdf.GeneratePdf(stream);
stream.Position = 0;
return File(stream.ToArray(), "application/pdf", "Report.pdf");
}

// 📌 Generazione report in EXCEL con ClosedXML
public async Task<IActionResult> GenerateExcel()
{
    var devices = await _context.Devices
        .Include(d => d.Interventions)
        .ToListAsync();

    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Report");

    // Definizione delle intestazioni del file Excel
    var headers = new[]
    {
        "Dispositivo", "Marca", "Modello", "S/N", "Collaudo",
        "Ultima Manutenzione", "Ultima Verifica Elettrica", "Ultimo Controllo Fisico",
        "Prossima Manutenzione", "Prossima Verifica Elettrica", "Prossimo Controllo Fisico"
    };

    // Formattazione della riga di intestazione
    var headerRange = worksheet.Range(1, 1, 1, headers.Length);
    headerRange.Style.Font.Bold = true;
    headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
    headerRange.Style.Font.FontColor = XLColor.White;
    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

    for (var i = 0; i < headers.Length; i++)
    {
        var cell = worksheet.Cell(1, i + 1);
        cell.Value = headers[i];
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    var row = 2;
    foreach (var device in devices)
    {
        var lastPreventiveMaintenance = device.Interventions
            .Where(i => i is { Type: InterventionType.Maintenance, MaintenanceCategory: MaintenanceType.Preventive })
            .OrderByDescending(i => i.Date)
            .FirstOrDefault();

        var lastElectricalTest = device.Interventions
            .Where(i => i.Type == InterventionType.ElectricalTest)
            .OrderByDescending(i => i.Date)
            .FirstOrDefault();

        var lastPhysicalInspection = device.Interventions
            .Where(i => i.Type == InterventionType.PhysicalInspection)
            .OrderByDescending(i => i.Date)
            .FirstOrDefault();

        var nextMaintenance = await _deadlineService.GetNextMaintenanceDueDateAsync(device);
        var nextElectricalTest = await _deadlineService.GetNextElectricalTestDueDateAsync(device);
        var nextPhysicalInspection = await _deadlineService.GetNextPhysicalInspectionDueDateAsync(device);

        for (var col = 1; col <= headers.Length; col++)
        {
            var cell = worksheet.Cell(row, col);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            if (row % 2 == 0) cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        worksheet.Cell(row, 1).Value = device.Name;
        worksheet.Cell(row, 2).Value = device.Brand;
        worksheet.Cell(row, 3).Value = device.Model;
        worksheet.Cell(row, 4).Value = device.SerialNumber;
        worksheet.Cell(row, 5).Value = device.DataCollaudo.ToShortDateString();
        worksheet.Cell(row, 6).Value = lastPreventiveMaintenance?.Date.ToShortDateString() ?? "N/A";
        worksheet.Cell(row, 7).Value = lastElectricalTest?.Date.ToShortDateString() ?? "N/A";
        worksheet.Cell(row, 8).Value = lastPhysicalInspection?.Date.ToShortDateString() ?? "N/A";
        worksheet.Cell(row, 9).Value = nextMaintenance?.ToShortDateString() ?? "N/A";
        worksheet.Cell(row, 10).Value = nextElectricalTest?.ToShortDateString() ?? "N/A";
        worksheet.Cell(row, 11).Value = nextPhysicalInspection?.ToShortDateString() ?? "N/A";

        row++;
    }

    worksheet.Columns().AdjustToContents();

    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    stream.Position = 0;
    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
}
}*/