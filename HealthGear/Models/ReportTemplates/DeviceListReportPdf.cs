using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace HealthGear.Models.ReportTemplates;

public static class DeviceListReportPdf
{
    public static byte[] Generate(List<Device> devices, string statusFilter)
    {
        // 🔹 Determina il titolo del report in base al filtro
        var reportTitle = statusFilter switch
        {
            "attivi" => "Report Dispositivi Attivi",
            "dismessi" => "Report Dispositivi Dismessi",
            _ => "Report Completo Dispositivi"
        };

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
                        row.AutoItem().Text($" {reportTitle}")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                    });

                    header.Item().AlignCenter()
                        .Text($"Generato il {DateTime.Now:dd/MM/yyyy} alle ore {DateTime.Now:T}");
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

                            deviceSection.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"{device.Name} - S/N: {device.SerialNumber}")
                                    .Italic().FontSize(10);

                                row.RelativeItem().AlignRight().Text($"Stato: {device.Status}")
                                    .Italic().FontSize(10);
                            });

                            deviceSection.Item().Height(5);
                            deviceSection.Item().LineHorizontal(1);
                            deviceSection.Item().Height(5);

                            // 📌 Data collaudo
                            deviceSection.Item().Row(row =>
                            {
                                row.ConstantItem(10).Image("Assets/Icons/calendar-check.png");
                                row.RelativeItem().Text($" Data collaudo: {device.DataCollaudo:dd/MM/yyyy}");
                            });

                            deviceSection.Item().Height(5);

                            // 📌 Ultima manutenzione e scadenza
                            deviceSection.Item().Row(row =>
                            {
                                row.ConstantItem(10).Image("Assets/Icons/wrench.png");
                                row.RelativeItem().Text(" Ultima manutenzione:").Bold();
                                row.RelativeItem().Text($"{lastMaintenance?.Date.ToString("dd/MM/yyyy") ?? "N/A"}");
                                row.ConstantItem(10).Image("Assets/Icons/calendar-x-mark.png");
                                row.RelativeItem().Text(" Scadenza:").Bold();
                                row.RelativeItem()
                                    .Text($"{device.NextMaintenanceDue?.ToString("dd/MM/yyyy") ?? "N/A"}");
                            });

                            // 📌 Ultima verifica elettrica e scadenza
                            deviceSection.Item().Row(row =>
                            {
                                row.ConstantItem(10).Image("Assets/Icons/bolt.png");
                                row.RelativeItem().Text(" Ultima verifica elettrica:").Bold();
                                row.RelativeItem().Text($"{lastElectricalTest?.Date.ToString("dd/MM/yyyy") ?? "N/A"}");
                                row.ConstantItem(10).Image("Assets/Icons/calendar-x-mark.png");
                                row.RelativeItem().Text(" Scadenza:").Bold();
                                row.RelativeItem()
                                    .Text($"{device.NextElectricalTestDue?.ToString("dd/MM/yyyy") ?? "N/A"}");
                            });

                            // 📌 Ultimo controllo fisico e scadenza da mostrare solo per i dispositivi radiologici e RM
                            if (device.RequiresPhysicalInspection)
                            {
                                deviceSection.Item().Row(row =>
                                {
                                    row.ConstantItem(10).Image("Assets/Icons/radiation.png");
                                    row.RelativeItem().Text(" Ultimo controllo fisico:").Bold();
                                    row.RelativeItem()
                                        .Text($"{lastPhysicalInspection?.Date.ToString("dd/MM/yyyy") ?? "N/A"}");
                                    row.ConstantItem(10).Image("Assets/Icons/calendar-x-mark.png");
                                    row.RelativeItem().Text(" Scadenza:").Bold();
                                    row.RelativeItem()
                                        .Text($"{device.NextPhysicalInspectionDue?.ToString("dd/MM/yyyy") ?? "N/A"}");
                                });
                            }
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
        return stream.ToArray();
    }
}