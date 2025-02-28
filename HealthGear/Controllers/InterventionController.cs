#region

using HealthGear.Data;
using HealthGear.Helpers;
using HealthGear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

#endregion

namespace HealthGear.Controllers;

[Route("Intervention")]
public class InterventionController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 10;

// 🔍 GET: /Intervention/Index/{deviceId}
// Mostra la lista degli interventi per un dispositivo specifico
    [HttpGet("Index/{deviceId:int}")]
    public async Task<IActionResult> Index(int deviceId, int page = 1)
    {
        // Recupera il dispositivo dal database con le informazioni necessarie
        var device = await context.Devices
            .Where(d => d.Id == deviceId)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Brand,
                d.Model,
                d.SerialNumber,
                d.InventoryNumber,
                d.FileAttachments
            })
            .FirstOrDefaultAsync();

        if (device == null)
            return NotFound("Dispositivo non trovato.");

        // Recupera la lista degli interventi ordinati per data decrescente
// Recupera gli interventi
        var interventionsList = await context.Interventions
            .Where(i => i.DeviceId == deviceId)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

// Calcola il totale degli interventi
        var totalItems = interventionsList.Count;

// Crea il ViewModel utilizzando una StaticPagedList (per ora impostiamo la pagina a 1 e pageSize uguale al totale, in assenza di paginazione attiva)
        var viewModel = new InterventionHistoryViewModel
        {
            DeviceId = device.Id,
            Interventions = new StaticPagedList<Intervention>(interventionsList, page, PageSize, totalItems)
        };

        //return View("~/Views/InterventionHistory/List.cshtml", viewModel);

        // Passa i dati del dispositivo tramite ViewBag per il titolo e altre informazioni
        ViewBag.DeviceId = device.Id;
        ViewBag.DeviceName = $"{device.Name} {device.Brand} {device.Model} (S/N: {device.SerialNumber})";
        ViewBag.DeviceBrand = device.Brand;
        ViewBag.DeviceModel = device.Model;
        ViewBag.DeviceSerialNumber = device.SerialNumber;
        ViewBag.DeviceInventoryNumber = device.InventoryNumber;

        return View("~/Views/InterventionHistory/List.cshtml", viewModel);
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

    [HttpPost("Create/{deviceId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int deviceId, Intervention intervention)
    {
        // Verifica che il deviceId sia valido
        if (deviceId <= 0)
            return BadRequest("DeviceId non valido.");

        // Recupera il dispositivo dal database per associarlo all'intervento
        var device = await context.Devices.FindAsync(deviceId);
        if (device == null)
            return NotFound("Dispositivo non trovato.");

        // Associa l'intervento al dispositivo
        intervention.DeviceId = deviceId;
        intervention.Device = device;

        // Se il modello non è valido, ritorna la view di creazione con i dati inseriti
        if (!ModelState.IsValid)
            return View("Create", intervention);

        // Reset dei campi opzionali in base al tipo di intervento
        intervention.MaintenanceCategory = intervention.Type == InterventionType.Maintenance
            ? intervention.MaintenanceCategory
            : null;
        intervention.Passed =
            intervention.Type is InterventionType.ElectricalTest or InterventionType.PhysicalInspection
                ? intervention.Passed
                : null;

        // Aggiunge l'intervento al contesto e salva, così da ottenere un ID generato dal database
        context.Interventions.Add(intervention);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Intervento aggiunto con successo! Ora puoi caricare i documenti relativi all’intervento oppure tornare ai dettagli del dispositivo.";

        // Dopo il salvataggio, l'intervento ha un ID valido. 
        // Reindirizza alla pagina dei dettagli dell'intervento, dove sarà disponibile la funzionalità di upload file.
        return RedirectToAction("Details", "Intervention", new { id = intervention.Id });
    }


    // ✨ GET: /Intervention/Details/{id}
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, string? returnUrl = null)
    {
        var intervention = await context.Interventions
            .Include(i => i.Device)
            .Include(i => i.Attachments) // Aggiungi questa Include per caricare i file
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null) return NotFound();

        // ✅ Se returnUrl è nullo, proviamo a dedurre la provenienza dall'header Referer
        if (string.IsNullOrEmpty(returnUrl))
        {
            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrEmpty(referer))
                returnUrl = referer.Contains("/InterventionHistory/List", StringComparison.OrdinalIgnoreCase)
                    ? Url.Action("List", "InterventionHistory", new { deviceId = intervention.DeviceId })
                    : Url.Action("Details", "Device", new { id = intervention.DeviceId });
        }

        // ✅ Se ancora null, fallback alla pagina dei dettagli del dispositivo
        ViewBag.ReturnUrl = returnUrl ?? Url.Action("Details", "Device", new { id = intervention.DeviceId });

        return View("Details", intervention);
    }

    // ✏️ GET: /Intervention/Edit/{id}
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, string? returnUrl = null)
    {
        var intervention = await context.Interventions
            .Include(i => i.Attachments).Include(intervention => intervention.Device)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null) return NotFound();

        ViewBag.SupportsPhysicalInspection =
            intervention.Device?.DeviceType is DeviceType.Radiologico or DeviceType.Mammografico;
        ViewBag.DeviceName =
            intervention.Device?.Name ?? "Dispositivo Sconosciuto"; // ✅ Passiamo il nome del dispositivo
        ViewBag.ReturnUrl =
            returnUrl ?? Url.Action("List", "InterventionHistory", new { deviceId = intervention.DeviceId });

        return View("Edit", intervention);
    }

// 📝 POST: /Intervention/Edit/{id}
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Intervention intervention, string? returnUrl = null)
    {
        if (id != intervention.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", intervention);

        var existingIntervention = await context.Interventions
            .Include(i => i.Device)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (existingIntervention == null) return NotFound();

        // Aggiorna i campi principali...
        existingIntervention.Date = intervention.Date;
        existingIntervention.PerformedBy = intervention.PerformedBy;
        existingIntervention.Notes = intervention.Notes;
        // Ecc.

        context.Update(existingIntervention);
        await context.SaveChangesAsync();

        // Se vuoi aggiornare il device per calcolare scadenze
        var device = existingIntervention.Device;
        if (device != null)
        {
            DueDateHelper.UpdateNextDueDate(device, context);
            context.Update(device);
            await context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Modifica salvata con successo!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("List", "InterventionHistory",
            new { deviceId = existingIntervention.DeviceId, deviceName = device?.Name });
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

        context.Interventions.Remove(intervention);
        await context.SaveChangesAsync();

        DueDateHelper.UpdateNextDueDate(device, context);

        var settings = await context.MaintenanceSettings.FirstOrDefaultAsync();
        if (settings == null) return StatusCode(500, "Errore nel recupero delle impostazioni di manutenzione.");

        if (device.Interventions.Count == 0)
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

        // 🔍 Log di Debug per verificare `returnUrl`
        var referer = Request.Headers.Referer.ToString();
        Console.WriteLine($"🔍 returnUrl: {returnUrl} | Referer: {referer}");

        // Se returnUrl è nullo, proviamo a dedurre la provenienza
        if (string.IsNullOrEmpty(returnUrl))
            if (!string.IsNullOrEmpty(referer))
            {
                if (referer.Contains("/InterventionHistory/List", StringComparison.OrdinalIgnoreCase))
                    returnUrl = Url.Action("List", "InterventionHistory", new { deviceId = device.Id });
                else if (referer.Contains("/Device/Details", StringComparison.OrdinalIgnoreCase))
                    returnUrl = Url.Action("Details", "Device", new { id = device.Id });
            }

        // Se abbiamo un returnUrl valido, reindirizziamo lì
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            //Console.WriteLine($"✅ Reindirizzamento a: {returnUrl}");
            return Redirect(returnUrl);

        // 🔄 Fallback: Torniamo ai dettagli del dispositivo se tutto il resto fallisce
        Console.WriteLine($"🛠 Eliminazione intervento {id} completata.");
        Console.WriteLine($"🔍 Reindirizzamento a Device/Details/{device.Id}");
        return RedirectToAction("Details", "Device", new { id = device.Id });
    }
}