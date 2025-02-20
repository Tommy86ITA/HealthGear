using ClosedXML.Excel;
using HealthGear.Models;

namespace HealthGear.Services.Reports.ReportTemplates;

public static class DeviceDetailsExcel
{
    public static byte[] Generate(Device device)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Dettagli Dispositivo");

        // 📌 Intestazione del Report
        worksheet.Cell("A1").Value = $"Report Dispositivo: {device.Brand} {device.Model}";
        worksheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(14);

        worksheet.Cell("A2").Value = $"Generato il {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        worksheet.Range("A2:E2").Merge().Style.Font.SetItalic();

        worksheet.Cell("A3").Value = "Dati Principali";
        worksheet.Range("A3:E3").Merge().Style.Font.SetBold().Font.SetFontSize(12);
        worksheet.Range("A3:E3").Style.Fill.BackgroundColor = XLColor.LightGray;
        
        worksheet.Style.Font.FontName = "Arial Unicode MS";

        // 📌 Dati principali del dispositivo
        var row = 4;
        worksheet.Cell(row, 1).Value = "Nome:";
        worksheet.Cell(row, 2).Value = device.Name;
        row++;

        worksheet.Cell(row, 1).Value = "Serial Number:";
        worksheet.Cell(row, 2).Value = device.SerialNumber;
        row++;

        worksheet.Cell(row, 1).Value = "Data Collaudo:";
        worksheet.Cell(row, 2).Value = device.DataCollaudo.ToShortDateString();
        row++;

        worksheet.Cell(row, 1).Value = "Scadenza Manutenzione:";
        worksheet.Cell(row, 2).Value = device.NextMaintenanceDue?.ToShortDateString() ?? "N/A";
        worksheet.Cell(row, 2).Style.Fill.BackgroundColor = GetStatusColor(device.NextMaintenanceDue);
        row++;

        worksheet.Cell(row, 1).Value = "Scadenza Verifica Elettrica:";
        worksheet.Cell(row, 2).Value = device.NextElectricalTestDue?.ToShortDateString() ?? "N/A";
        worksheet.Cell(row, 2).Style.Fill.BackgroundColor = GetStatusColor(device.NextElectricalTestDue);
        row++;

        if (device.RequiresPhysicalInspection)
        {
            worksheet.Cell(row, 1).Value = "Scadenza Controlli Fisici:";
            worksheet.Cell(row, 2).Value = device.NextPhysicalInspectionDue?.ToShortDateString() ?? "N/A";
            worksheet.Cell(row, 2).Style.Fill.BackgroundColor = GetStatusColor(device.NextPhysicalInspectionDue);
            row++;
        }

        // 📌 Storico interventi
        row += 2;
        worksheet.Cell(row, 1).Value = "Storico Interventi";
        worksheet.Range(row, 1, row, 4).Merge().Style.Font.SetBold().Font.SetFontSize(12);
        worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;

        // Intestazione tabella
        worksheet.Cell(row, 1).Value = "Data";
        worksheet.Cell(row, 2).Value = "Tipo Intervento";
        worksheet.Cell(row, 3).Value = "Eseguito da";
        worksheet.Cell(row, 4).Value = "Esito";

        worksheet.Range(row, 1, row, 4).Style.Font.SetBold();
        worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
        row++;

        // Dati della tabella
        foreach (var intervention in device.Interventions.OrderByDescending(i => i.Date))
        {
            worksheet.Cell(row, 1).Value = intervention.Date.ToShortDateString();
            worksheet.Cell(row, 2).Value = GetInterventionDisplayName(intervention);
            worksheet.Cell(row, 3).Value = intervention.PerformedBy;
            worksheet.Cell(row, 4).Value = intervention.Passed switch
            {
                true => "Superato",
                false => "Non superato",
                _ => "N/A"
            };

            // Colorazione esito
            worksheet.Cell(row, 4).Style.Fill.BackgroundColor =
                intervention.Passed == false ? XLColor.Red : XLColor.LightGreen;

            row++;
        }

        worksheet.Columns().AdjustToContents(); // Adatta le colonne

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // 🔹 Metodo per determinare il colore dello stato della scadenza
    private static XLColor GetStatusColor(DateTime? dueDate)
    {
        if (!dueDate.HasValue) return XLColor.LightGray;
        var daysRemaining = (dueDate.Value - DateTime.Today).TotalDays;
        return daysRemaining < 0 ? XLColor.Red :
            daysRemaining <= 60 ? XLColor.Orange :
            XLColor.LightGreen;
    }

    // 🔹 Ottieni il nome visualizzato dell'intervento
    private static string GetInterventionDisplayName(Intervention intervention)
    {
        if (intervention.Type == InterventionType.Maintenance)
            return intervention.MaintenanceCategory switch
            {
                MaintenanceType.Preventive => "Manutenzione - Preventiva",
                MaintenanceType.Corrective => "Manutenzione - Correttiva",
                _ => "Manutenzione"
            };

        return intervention.Type.ToString();
    }
}