#region

using HealthGear.Data;
using HealthGear.Models;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HealthGear.Services;

public class DeadlineService
{
    private readonly ApplicationDbContext _context;

    public DeadlineService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Ottiene le impostazioni di manutenzione dal database.
    /// </summary>
    private async Task<MaintenanceSettings> GetSettingsAsync()
    {
        var settings = await _context.MaintenanceSettings.FirstOrDefaultAsync();
        if (settings != null) return settings;
        settings = new MaintenanceSettings();
        _context.MaintenanceSettings.Add(settings);
        await _context.SaveChangesAsync();

        return settings;
    }

    /// <summary>
    ///     Calcola la prossima scadenza per la manutenzione ordinaria di un dispositivo.
    /// </summary>
    public async Task<DateTime?> GetNextMaintenanceDueDateAsync(Device device)
    {
        var settings = await GetSettingsAsync();

        var referenceDate = device.LastOrdinaryMaintenance.HasValue &&
                            device.LastOrdinaryMaintenance.Value > DateTime.MinValue
            ? device.LastOrdinaryMaintenance.Value
            : device.DataCollaudo;

        if (referenceDate == DateTime.MinValue)
        {
            Console.WriteLine($"[DEBUG] {device.Name} - Invalid reference date for maintenance, returning null.");
            return null;
        }

        var nextDueDate = referenceDate.AddMonths(settings.MaintenanceIntervalMonths);
        Console.WriteLine($"[DEBUG] {device.Name} - Calculated Next Maintenance Due: {nextDueDate}");
        return nextDueDate;
    }

    /// <summary>
    ///     Calcola la prossima scadenza per la verifica elettrica.
    /// </summary>
    public async Task<DateTime?> GetNextElectricalTestDueDateAsync(Device device)
    {
        var settings = await GetSettingsAsync();

        var lastElectricalTest = device.Interventions?
            .Where(i => i.Type == InterventionType.ElectricalTest)
            .OrderByDescending(i => i.Date)
            .Select(i => i.Date)
            .FirstOrDefault();

        var referenceDate = lastElectricalTest.HasValue && lastElectricalTest.Value > DateTime.MinValue
            ? lastElectricalTest.Value
            : device.FirstElectricalTest;

        if (referenceDate == DateTime.MinValue)
        {
            Console.WriteLine($"[DEBUG] {device.Name} - Invalid reference date for electrical test, returning null.");
            return null;
        }

        var nextDueDate = referenceDate.AddYears(settings.ElectricalTestIntervalYears);
        Console.WriteLine($"[DEBUG] {device.Name} - Calculated Next Electrical Test Due: {nextDueDate}");
        return nextDueDate;
    }

    /// <summary>
    ///     Calcola la prossima scadenza per il controllo fisico.
    /// </summary>
    /// <summary>
    ///     Calcola la prossima scadenza per il controllo fisico.
    /// </summary>
    public async Task<DateTime?> GetNextPhysicalInspectionDueDateAsync(Device device)
    {
        var settings = await GetSettingsAsync();

        // ✅ Escludiamo tutti i dispositivi che non sono radiologici o mammografici
        if (device.DeviceType != DeviceType.Radiologico && device.DeviceType != DeviceType.Mammografico)
        {
            Console.WriteLine($"[DEBUG] {device.Name} - Nessuna verifica fisica necessaria ({device.DeviceType})");
            return null;
        }

        // ✅ Determiniamo l'intervallo corretto: annuale per radiologici, semestrale per mammografi
        var intervalMonths = device.DeviceType == DeviceType.Mammografico
            ? settings.MammographyInspectionIntervalMonths // Semestrale per mammografi
            : settings.PhysicalInspectionIntervalMonths; // Annuale per gli altri radiologici

        // ✅ Troviamo l'ultima verifica fisica effettuata, altrimenti prendiamo la data di collaudo o la prima verifica
        var lastPhysicalInspection = device.Interventions?
            .Where(i => i is { Type: InterventionType.PhysicalInspection, Passed: true })
            .OrderByDescending(i => i.Date)
            .Select(i => (DateTime?)i.Date)
            .FirstOrDefault();

        // Se non ci sono interventi, usiamo la prima verifica fisica o la data di collaudo come riferimento
        if (!lastPhysicalInspection.HasValue || lastPhysicalInspection.Value == DateTime.MinValue)
            lastPhysicalInspection = device.FirstPhysicalInspection ?? device.DataCollaudo;

        // Se il valore è ancora MinValue dopo questa assegnazione, significa che non esiste nessun riferimento valido.
        if (!lastPhysicalInspection.HasValue || lastPhysicalInspection.Value == DateTime.MinValue)
        {
            Console.WriteLine($"[DEBUG] {device.Name} - Nessuna data valida per verifica fisica, returning null.");
            return null;
        }

        // ✅ Calcoliamo la prossima verifica fisica
        var nextDueDate = lastPhysicalInspection.Value.AddMonths(intervalMonths);

        Console.WriteLine(
            $"[DEBUG] {device.Name} - Ultima verifica fisica: {lastPhysicalInspection.Value.ToShortDateString()}, Prossima: {nextDueDate.ToShortDateString()}");

        return nextDueDate;
    }

    /// <summary>
    ///     Aggiorna le scadenze nel database.
    /// </summary>
    public async Task UpdateNextDueDatesAsync(Device device)
    {
        Console.WriteLine($"[DEBUG] Updating due dates for device: {device.Name}");

        var settings = await GetSettingsAsync();

        device.NextMaintenanceDue = await GetNextMaintenanceDueDateAsync(device);
        device.NextElectricalTestDue = await GetNextElectricalTestDueDateAsync(device);
        device.NextPhysicalInspectionDue = await GetNextPhysicalInspectionDueDateAsync(device);

        Console.WriteLine($"[DEBUG] {device.Name} - Final Next Maintenance Due: {device.NextMaintenanceDue}");
        Console.WriteLine($"[DEBUG] {device.Name} - Final Next Electrical Test Due: {device.NextElectricalTestDue}");
        Console.WriteLine(
            $"[DEBUG] {device.Name} - Final Next Physical Inspection Due: {device.NextPhysicalInspectionDue}");

        _context.Entry(device).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        Console.WriteLine($"[DEBUG] {device.Name} - Due dates successfully updated in database.");
    }
}