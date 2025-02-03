using HealthGear.Data;
using HealthGear.Helpers;
using HealthGear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

[Route("Intervention")]
public class InterventionController(ApplicationDbContext context) : Controller
{
    // 🔍 GET: /Intervention/Index/{deviceId}
    // Mostra la lista degli interventi per un dispositivo specifico
    [HttpGet("Index/{deviceId:int}")]
    public async Task<IActionResult> Index(int deviceId)
    {
        var interventions = await context.Interventions
            .Where(i => i.DeviceId == deviceId)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

        return View("~/Views/InterventionHistory/List.cshtml", interventions);
    }

    // ➕ GET: /Intervention/Create/{deviceId}
    [HttpGet("Create/{deviceId:int}")]
    public async Task<IActionResult> Create(int deviceId)
    {
        var device = await context.Devices.FindAsync(deviceId);
        if (device == null) return NotFound();

        ViewBag.SupportsPhysicalInspection = device.DeviceType is DeviceType.Radiologico or DeviceType.Mammografico;

        var intervention = new Intervention { DeviceId = deviceId, Date = DateTime.Now };
        return View("Create", intervention);
    }

    // 📝 POST: /Intervention/Create/{deviceId}
    [HttpPost("Create/{deviceId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int deviceId, Intervention intervention)
    {
        if (deviceId <= 0) return BadRequest("DeviceId non valido.");

        var device = await context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null) return NotFound();

        intervention.DeviceId = deviceId;
        intervention.Device = device;

        if (!ModelState.IsValid) return View("Create", intervention);

        // 🛠 Reset dei campi opzionali in base al tipo di intervento
        intervention.MaintenanceCategory = intervention.Type == InterventionType.Maintenance ? intervention.MaintenanceCategory : null;
        intervention.Passed = intervention.Type is InterventionType.ElectricalTest or InterventionType.PhysicalInspection ? intervention.Passed : null;

        // 💾 Salviamo l'intervento
        context.Interventions.Add(intervention);
        await context.SaveChangesAsync();

        // 🔄 Aggiorniamo le scadenze del dispositivo
        DueDateHelper.UpdateNextDueDate(device, context);
        context.Update(device);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Intervento aggiunto con successo!";
        return RedirectToAction("Details", "Device", new { id = deviceId });
    }
    
    // ✏️ GET: /Intervention/Details/{id}
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var intervention = await context.Interventions
            .Include(i => i.Device)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null) return NotFound();

        return View("Details", intervention);
    }

    // ✏️ GET: /Intervention/Edit/{id}
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, string? returnUrl = null)
    {
        var intervention = await context.Interventions.FindAsync(id);
        if (intervention == null) return NotFound();

        ViewBag.SupportsPhysicalInspection = await context.Devices
            .Where(d => d.Id == intervention.DeviceId)
            .Select(d => d.DeviceType == DeviceType.Radiologico || d.DeviceType == DeviceType.Mammografico)
            .FirstOrDefaultAsync();

        // Se returnUrl è nullo, torniamo alla lista completa degli interventi del dispositivo
        ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Intervention", new { deviceId = intervention.DeviceId });

        return View("Edit", intervention);
    }

    // 📝 POST: /Intervention/Edit/{id}
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Intervention intervention, string? returnUrl = null)
    {
        if (id != intervention.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", intervention);

        var existingIntervention = await context.Interventions.FindAsync(id);
        if (existingIntervention == null) return NotFound();

        // 🔄 Aggiorniamo i campi
        existingIntervention.Date = intervention.Date;
        existingIntervention.PerformedBy = intervention.PerformedBy;
        existingIntervention.Notes = intervention.Notes;

        if (existingIntervention.Type == InterventionType.Maintenance)
            existingIntervention.MaintenanceCategory = intervention.MaintenanceCategory;
        else
            existingIntervention.MaintenanceCategory = null; // Reset se non è manutenzione

        if (existingIntervention.Type is InterventionType.ElectricalTest or InterventionType.PhysicalInspection)
            existingIntervention.Passed = intervention.Passed;
        else
            existingIntervention.Passed = null; // Reset se non è test elettrico/fisico

        context.Update(existingIntervention);
        await context.SaveChangesAsync();

        var device = await context.Devices.Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == intervention.DeviceId);

        if (device != null)
        {
            DueDateHelper.UpdateNextDueDate(device, context);
            context.Update(device);
            await context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Modifica salvata con successo!";

// Se returnUrl è presente e valido, lo usiamo per il redirect
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

// Se l'utente proviene dallo storico interventi (es. /InterventionHistory/List), torniamo lì
        if (Request.Headers["Referer"].ToString().Contains("/InterventionHistory/List"))
        {
            return RedirectToAction("List", "InterventionHistory", new { deviceId = intervention.DeviceId });
        }

// Se nessuna delle precedenti condizioni è soddisfatta, torniamo alla lista degli interventi
        return RedirectToAction("Index", "Intervention", new { deviceId = intervention.DeviceId });
    }

    // 🗑️ POST: /Intervention/Delete/{id}
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        var intervention = await context.Interventions.FindAsync(id);
        if (intervention == null) return NotFound();

        var device = await context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == intervention.DeviceId);

        if (device == null) return NotFound();

        // 💾 Rimuoviamo l'intervento e salviamo
        context.Interventions.Remove(intervention);
        await context.SaveChangesAsync();

        // 🔄 Ricalcoliamo le scadenze
        DueDateHelper.UpdateNextDueDate(device, context);

        // 🔄 Se il dispositivo non ha più interventi, ripristiniamo le date iniziali
        var settings = await context.MaintenanceSettings.FirstOrDefaultAsync();
        if (settings == null) return StatusCode(500, "Errore nel recupero delle impostazioni di manutenzione.");

        if (!device.Interventions.Any())
        {
            device.NextMaintenanceDue = device.DataCollaudo.AddMonths(settings.MaintenanceIntervalMonths);
            device.NextElectricalTestDue = device.FirstElectricalTest.AddMonths(settings.ElectricalTestIntervalMonths);
            device.NextPhysicalInspectionDue = device.FirstPhysicalInspection?.AddMonths(
                device.DeviceType == DeviceType.Mammografico
                    ? settings.MammographyInspectionIntervalMonths
                    : settings.PhysicalInspectionIntervalMonths);
        }

        context.Update(device);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Intervento eliminato con successo!";
        return Redirect(returnUrl ?? Url.Action("Index", "Intervention", new { deviceId = device.Id }));
    }
}