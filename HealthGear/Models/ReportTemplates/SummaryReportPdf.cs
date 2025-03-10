using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace HealthGear.Models.ReportTemplates;

public static class SummaryReportPdf
{
    public static byte[] Generate(List<Device> devices)
    {
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); // Imposta l'orientamento orizzontale
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(7)); // Ridotto il font per ottimizzare lo spazio

                // 📌 Intestazione del report
                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Row(row =>
                    {
                        row.AutoItem().Width(15).Image("Assets/Icons/clipboard.png");
                        row.AutoItem().Text(" Report Riepilogativo Dispositivi")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                    });

                    header.Item().AlignCenter()
                        .Text($"Generato il {DateTime.Now:dd/MM/yyyy} alle ore {DateTime.Now:T}");
                    header.Item().AlignRight().Text("Made with HealthGear v. 1.0");
                });

                // 📌 Contenuto della tabella
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Nome dispositivo
                        columns.RelativeColumn(2); // Brand
                        columns.RelativeColumn(2); // Modello
                        columns.RelativeColumn(3); // S/N - Larghezza aumentata
                        columns.RelativeColumn(2); // Data collaudo
                        columns.RelativeColumn(2); // Ultima manutenzione preventiva
                        columns.RelativeColumn(2); // Scadenza prossima manutenzione preventiva
                        columns.RelativeColumn(2); // Ultima verifica elettrica
                        columns.RelativeColumn(2); // Scadenza prossima verifica elettrica
                        columns.RelativeColumn(2); // Ultimo controllo fisico
                        columns.RelativeColumn(2); // Scadenza prossimo controllo fisico
                        columns.RelativeColumn(4); // Note
                    });

                    // 📌 Intestazioni della tabella
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("Nome Dispositivo")
                            .Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("Brand").Bold()
                            .FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("Modello").Bold()
                            .FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("S/N").Bold()
                            .FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("Data Collaudo")
                            .Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("Ultima Man. Prev.")
                            .Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter()
                            .Text("Scad. Prox Man. Prev.").Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("Ultima Ver. Elettr.")
                            .Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter()
                            .Text("Scad. Prox Ver. Elettr.").Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter()
                            .Text("Ultimo Controllo Fisico").Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter()
                            .Text("Scad. Prox Controllo Fisico").Bold().FontSize(9).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Medium).Border(1).AlignCenter().Text("Note").Bold()
                            .FontSize(9).FontColor(Colors.White);
                    });

                    // 📌 Riempimento della tabella con i dati dei dispositivi
                    for (var i = 0; i < devices.Count; i++)
                    {
                        var device = devices[i];
                        var backgroundColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten3;

                        table.Cell().Background(backgroundColor).Border(1).Padding(5).Text(device.Name);
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).Text(device.Brand);
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).Text(device.Model);
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignLeft()
                            .Text(device.SerialNumber);
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter()
                            .Text(device.DataCollaudo.ToString("dd/MM/yyyy"));
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter()
                            .Text(device.LastOrdinaryMaintenance?.ToString("dd/MM/yyyy") ?? "⚠️");
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter()
                            .Text(device.NextMaintenanceDue?.ToString("dd/MM/yyyy") ?? "⚠️");
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter().Text(
                            device.LastElectricalTest?.ToString("dd/MM/yyyy") == "01/01/0001"
                                ? "N/A"
                                : device.LastElectricalTest?.ToString("dd/MM/yyyy") ?? "N/A");
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter()
                            .Text(device.NextElectricalTestDue?.ToString("dd/MM/yyyy") ?? "N/A");
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter().Text(
                            device.RequiresPhysicalInspection
                                ? device.LastPhysicalInspection.HasValue &&
                                  device.LastPhysicalInspection.Value != DateTime.MinValue
                                    ? device.LastPhysicalInspection.Value.ToString("dd/MM/yyyy")
                                    : device.FirstPhysicalInspection.HasValue
                                        ? device.FirstPhysicalInspection.Value.ToString("dd/MM/yyyy")
                                        : "⚠️"
                                : "Non soggetto"
                        );
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter().Text(
                            device.RequiresPhysicalInspection
                                ? device.NextPhysicalInspectionDue?.ToString("dd/MM/yyyy") ?? "⚠️"
                                : "Non soggetto"
                        );
                        table.Cell().Background(backgroundColor).Border(1).Padding(5).AlignCenter()
                            .Text(device.Notes ?? "--");
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