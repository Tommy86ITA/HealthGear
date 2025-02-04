#region

using HealthGear.Data;
using HealthGear.Models;

#endregion

namespace HealthGear.Helpers;

public static class DueDateHelper
{
    /// <summary>
    ///     Restituisce una classe CSS in base allo stato della scadenza.
    /// </summary>
    public static string GetDueDateClass(DateTime? dueDate)
    {
        if (!dueDate.HasValue) return "text-danger font-weight-bold"; // 🔴 Nessuna scadenza registrata
        var due = dueDate.Value;
        var today = DateTime.Today;
        var twoMonthsLater = today.AddMonths(2);

        if (due < today)
            return "text-danger font-weight-bold"; // 🔴 Scaduto
        if (due < twoMonthsLater)
            return "text-warning"; // 🟡 In scadenza
        return "text-success"; // 🟢 OK
    }

    /// <summary>
    ///     Restituisce la data di scadenza in formato leggibile, oppure un messaggio di avviso se mancante.
    /// </summary>
    public static string GetDueDateText(DateTime? dueDate)
    {
        return dueDate.HasValue
            ? dueDate.Value.ToString("dd/MM/yyyy")
            : "⚠ Nessuna scadenza registrata";
    }

    /// <summary>
    ///     Aggiorna le prossime scadenze di manutenzione, verifiche elettriche e controlli fisici in base all'ultimo
    ///     intervento registrato.
    ///     Se non ci sono interventi, ripristina la scadenza basandosi sulle date iniziali.
    /// </summary>
    public static void UpdateNextDueDate(Device device, ApplicationDbContext context)
    {
        var settings = context.MaintenanceSettings.FirstOrDefault();
        if (settings == null)
        {
            Console.WriteLine("[ERROR] MaintenanceSettings non trovato! Impossibile aggiornare le scadenze.");
            return;
        }

        // 🔧 TROVA L'ULTIMA MANUTENZIONE REGISTRATA
        var lastMaintenance = context.Interventions
            .Where(i => i.DeviceId == device.Id && i.Type == InterventionType.Maintenance)
            .OrderByDescending(i => i.Date)
            .FirstOrDefault();

        // ✅ Aggiorna la prossima scadenza della manutenzione
        device.NextMaintenanceDue = lastMaintenance != null
            ? lastMaintenance.Date.AddMonths(settings.MaintenanceIntervalMonths)
            : device.DataCollaudo.AddMonths(settings.MaintenanceIntervalMonths);

        // ⚡ TROVA L'ULTIMA VERIFICA ELETTRICA SUPERATA
        var lastElectricalTest = context.Interventions
            .Where(i => i.DeviceId == device.Id && i.Type == InterventionType.ElectricalTest && i.Passed == true)
            .OrderByDescending(i => i.Date)
            .FirstOrDefault();

        // ✅ Aggiorna la prossima scadenza della verifica elettrica
        device.NextElectricalTestDue = lastElectricalTest != null
            ? lastElectricalTest.Date.AddMonths(settings.ElectricalTestIntervalMonths)
            : device.FirstElectricalTest.AddMonths(settings.ElectricalTestIntervalMonths);

        // ☢️ TROVA L'ULTIMO CONTROLLO FISICO SUPERATO
        var lastPhysicalInspection = context.Interventions
            .Where(i => i.DeviceId == device.Id && i.Type == InterventionType.PhysicalInspection && i.Passed == true)
            .OrderByDescending(i => i.Date)
            .FirstOrDefault();

        // ✅ Aggiorna la prossima scadenza del controllo fisico
        device.NextPhysicalInspectionDue = lastPhysicalInspection != null
            ? lastPhysicalInspection.Date.AddMonths(
                device.DeviceType == DeviceType.Mammografico
                    ? settings.MammographyInspectionIntervalMonths
                    : settings.PhysicalInspectionIntervalMonths)
            : device.FirstPhysicalInspection?.AddMonths(
                device.DeviceType == DeviceType.Mammografico
                    ? settings.MammographyInspectionIntervalMonths
                    : settings.PhysicalInspectionIntervalMonths);

        // 💾 Salva le modifiche nel database
        context.Update(device);
        context.SaveChanges();
    }
}