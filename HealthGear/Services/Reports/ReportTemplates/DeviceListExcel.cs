using ClosedXML.Excel;
using HealthGear.Models;

namespace HealthGear.Services.Reports.ReportTemplates
{
    public static class DeviceListExcel
    {
        public static byte[] Generate(List<Device> devices, DeadlineService deadlineService)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report Dispositivi");

            var headers = new[]
            {
                "Dispositivo", "Marca", "Modello", "S/N", "Collaudo",
                "Ultima Manutenzione", "Ultima Verifica Elettrica", "Ultimo Controllo Fisico",
                "Prossima Manutenzione", "Prossima Verifica Elettrica", "Prossimo Controllo Fisico"
            };

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

                var nextMaintenance = deadlineService.GetNextMaintenanceDueDateAsync(device).Result;
                var nextElectricalTest = deadlineService.GetNextElectricalTestDueDateAsync(device).Result;
                var nextPhysicalInspection = deadlineService.GetNextPhysicalInspectionDueDateAsync(device).Result;

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

                // 📌 Applica la colorazione alle scadenze
                ApplyDateColoring(worksheet.Cell(row, 9), nextMaintenance);
                ApplyDateColoring(worksheet.Cell(row, 10), nextElectricalTest);
                ApplyDateColoring(worksheet.Cell(row, 11), nextPhysicalInspection);

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Colora la cella in base alla scadenza:
        /// - 🔴 Rosso → Scaduto
        /// - 🟡 Giallo → In scadenza nei prossimi 2 mesi
        /// - 🟢 Verde → Oltre 2 mesi dalla scadenza
        /// </summary>
        private static void ApplyDateColoring(IXLCell cell, DateTime? dueDate)
        {
            if (dueDate == null || dueDate == DateTime.MinValue)
            {
                cell.Value = "N/A";
                return;
            }

            cell.Value = dueDate.Value.ToShortDateString();

            if (dueDate < DateTime.Today)
                cell.Style.Fill.BackgroundColor = XLColor.Red; // 🔴 Scaduto
            else if (dueDate <= DateTime.Today.AddMonths(2))
                cell.Style.Fill.BackgroundColor = XLColor.Yellow; // 🟡 In scadenza
            else
                cell.Style.Fill.BackgroundColor = XLColor.Green; // 🟢 OK
        }
    }
}