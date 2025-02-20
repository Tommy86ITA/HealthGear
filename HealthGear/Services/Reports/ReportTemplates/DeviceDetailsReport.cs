using System.ComponentModel.DataAnnotations;
using HealthGear.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HealthGear.Services.Reports.ReportTemplates;

public static class DeviceDetailsReport
{
    public static byte[] Generate(Device device)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                // 📌 Intestazione del Report
                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Row(row =>
                    {
                        row.AutoItem().Width(15).Image("Assets/Icons/clipboard.png");
                        row.AutoItem().Text($" Report Dispositivo: {device.Brand} {device.Model}")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                    });

                    header.Item().PaddingBottom(10).AlignCenter()
                        .Text($"Generato il {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                    header.Item().AlignRight().Text("Made with HealthGear v. 1.0");
                    header.Item().PaddingBottom(5);
                    header.Item().LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    header.Item().PaddingBottom(5);
                });

                // 📌 Dati principali del dispositivo
                page.Content().Column(content =>
                {
                    content.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text($"Nome: {device.Name}").Bold();
                            left.Item().Text($"Serial Number: {device.SerialNumber}");
                            left.Item().Text($"Data di collaudo: {device.DataCollaudo:dd/MM/yyyy}");
                        });

                        row.RelativeItem().AlignRight().Column(right =>
                        {
                            right.Item().Text($"Scadenza manutenzione: {device.NextMaintenanceDue:dd/MM/yyyy}");
                            right.Item()
                                .Text($"Scadenza verifica elettrica: {device.NextElectricalTestDue:dd/MM/yyyy}");
                            if (device.RequiresPhysicalInspection)
                                right.Item()
                                    .Text($"Scadenza controlli fisici: {device.NextPhysicalInspectionDue:dd/MM/yyyy}");
                        });
                    });
                    content.Item().PaddingBottom(10).AlignCenter();
                    content.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // 📌 Storico Interventi
                    content.Item().AlignCenter();
                    content.Item().PaddingTop(10).Row(row =>
                    {
                        row.ConstantItem(15).Image("Assets/Icons/clock.png");
                        row.RelativeItem().Text(" Storico Interventi").Bold().FontSize(14);
                    });

                    content.Item().Table(table =>
                    {
                        // Definizione delle colonne
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75); // Data
                            columns.RelativeColumn(); // Tipo intervento
                            columns.ConstantColumn(120); // Eseguito da
                            columns.ConstantColumn(70); // Esito
                        });

                        // Intestazione della tabella
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text(text => text.Span("Data").Bold());
                            header.Cell().Element(CellStyle).Text(text => text.Span("Tipo Intervento").Bold());
                            header.Cell().Element(CellStyle).Text(text => text.Span("Eseguito da").Bold());
                            header.Cell().Element(CellStyle).Text(text => text.Span("Esito").Bold());
                            return;

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.Padding(5).BorderBottom(1).Background(Colors.Grey.Lighten3);
                            }
                        });

                        // Dati della tabella
                        foreach (var intervention in device.Interventions.OrderByDescending(i => i.Date))
                        {
                            table.Cell().Element(CellDataStyle).Text($"{intervention.Date:dd/MM/yyyy}");

                            // Tipo intervento con icona e spaziatura migliorata
                            table.Cell().Element(CellDataStyle).Row(row =>
                            {
                                row.Spacing(10);
                                row.ConstantItem(10).Image($"Assets/Icons/{GetInterventionIcon(intervention.Type)}");
                                row.RelativeItem().PaddingLeft(5).Text(GetInterventionDisplayName(intervention));
                            });

                            table.Cell().Element(CellDataStyle).Text(intervention.PerformedBy);

                            // Esito con colore dinamico
                            table.Cell().Element(CellDataStyle).Text(intervention.Passed switch
                            {
                                true => "Superato",
                                false => "Non superato",
                                _ => "N/A"
                            }).FontColor(intervention.Passed == false ? Colors.Red.Medium : Colors.Green.Medium);
                            
                            // 📌 **Aggiunta delle note sotto l'intervento (solo se presenti)**
                            table.Cell().ColumnSpan(4).PaddingTop(5).BorderBottom(1).Row(row =>
                            {
                                row.ConstantItem(8).Image("Assets/Icons/note-sticky-regular.png").FitWidth();
                                row.RelativeItem().Text($" Note: {intervention.Notes}")
                                    .FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
                            });

                            continue;

                            static IContainer CellDataStyle(IContainer container)
                            {
                                return container.Padding(7).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        }
                    });
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
        }).GeneratePdf();
    }

    // 🔹 Ottieni il nome visualizzato dell'enum (es. "Manutenzione - Preventiva")
    private static string GetInterventionDisplayName(Intervention intervention)
    {
        if (intervention.Type == InterventionType.Maintenance)
            return intervention.MaintenanceCategory switch
            {
                MaintenanceType.Preventive => "Manutenzione - Preventiva",
                MaintenanceType.Corrective => "Manutenzione - Correttiva",
                _ => "Manutenzione"
            };

        var field = intervention.Type.GetType().GetField(intervention.Type.ToString());
        var attribute = field is not null
            ? (DisplayAttribute?)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute))
            : null;
        return attribute?.Name ?? intervention.Type.ToString();
    }

    // 🔹 Ottieni l'icona corrispondente al tipo di intervento
    private static string GetInterventionIcon(InterventionType type)
    {
        return type switch
        {
            InterventionType.Maintenance => "wrench.png",
            InterventionType.ElectricalTest => "bolt.png",
            InterventionType.PhysicalInspection => "radiation.png",
            _ => "clipboard.png"
        };
    }
}