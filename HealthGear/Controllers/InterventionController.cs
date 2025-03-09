#region

using HealthGear.Constants;
using HealthGear.Data;
using HealthGear.Helpers;
using HealthGear.Models;
using HealthGear.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

#endregion

namespace HealthGear.Controllers;

[Route("Intervention")]
public class InterventionController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 10;

    /// <summary>
    ///     Mostra la lista degli interventi per un dispositivo specifico.
    ///     Accessibile a tutti i ruoli: Admin, Tecnico e Office.
    ///     La lista è paginata e ordinata per data decrescente.
    /// </summary>
    /// <param name="deviceId">ID del dispositivo</param>
    /// <param name="page">Pagina corrente (di default la prima)</param>
    /// <returns>La vista con lo storico degli interventi del dispositivo.</returns>
    [HttpGet("Index/{deviceId:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico + "," + Roles.Office)]
    public async Task<IActionResult> Index(int deviceId, int page = 1)
    {
        // ✅ Recupera le informazioni essenziali sul dispositivo
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

        // ✅ Recupera gli interventi associati al dispositivo, ordinati per data decrescente
        var interventionsList = await context.Interventions
            .Where(i => i.DeviceId == deviceId)
            .OrderByDescending(i => i.Date)
            .ToListAsync();

        // ✅ Calcola il numero totale di interventi (per la paginazione)
        var totalItems = interventionsList.Count;

        // ✅ Costruisce il ViewModel per la vista, utilizzando StaticPagedList
        var viewModel = new InterventionHistoryViewModel
        {
            DeviceId = device.Id,
            Interventions = new StaticPagedList<Intervention>(interventionsList, page, PageSize, totalItems)
        };

        // ✅ Passa informazioni aggiuntive sul dispositivo alla View tramite ViewBag
        ViewBag.DeviceId = device.Id;
        ViewBag.DeviceName = $"{device.Name} {device.Brand} {device.Model} (S/N: {device.SerialNumber})";
        ViewBag.DeviceBrand = device.Brand;
        ViewBag.DeviceModel = device.Model;
        ViewBag.DeviceSerialNumber = device.SerialNumber;
        ViewBag.DeviceInventoryNumber = device.InventoryNumber;

        // ✅ Ritorna la vista della cronologia interventi
        return View("~/Views/InterventionHistory/List.cshtml", viewModel);
    }

    /// <summary>
    ///     Mostra il form di creazione di un nuovo intervento per un dispositivo specifico.
    ///     Accessibile solo a Admin e Tecnico.
    /// </summary>
    /// <param name="deviceId">ID del dispositivo su cui creare l'intervento</param>
    /// <returns>La vista di creazione intervento</returns>
    [HttpGet("Create/{deviceId:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Create(int deviceId)
    {
        // ✅ Verifica che il dispositivo esista
        var device = await context.Devices.FindAsync(deviceId);
        if (device == null)
            return NotFound("Dispositivo non trovato.");

        // ✅ Passa alla view se il dispositivo richiede controlli fisici (solo per alcune tipologie)
        ViewBag.SupportsPhysicalInspection = device.DeviceType is DeviceType.Radiologico or DeviceType.Mammografico;

        // ✅ Precompila il modello con il DeviceId e la data odierna
        var intervention = new Intervention
        {
            DeviceId = deviceId,
            Date = DateTime.Now
        };

        return View("Create", intervention);
    }

    /// <summary>
    ///     Salva un nuovo intervento associato a un dispositivo specifico.
    ///     Accessibile solo a Admin e Tecnico.
    /// </summary>
    /// <param name="deviceId">ID del dispositivo associato</param>
    /// <param name="intervention">Dati dell'intervento</param>
    /// <returns>Redirect ai dettagli intervento o alla schermata di errore</returns>
    [HttpPost("Create/{deviceId:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Create(int deviceId, Intervention intervention)
    {
        // ✅ Verifica che il DeviceId sia valido
        if (deviceId <= 0)
            return BadRequest("DeviceId non valido.");

        // ✅ Recupera il dispositivo e associa al modello
        var device = await context.Devices.FindAsync(deviceId);
        if (device == null)
            return NotFound("Dispositivo non trovato.");

        intervention.DeviceId = deviceId;
        intervention.Device = device;

        // ✅ Validazione dei dati
        if (!ModelState.IsValid)
        {
            ViewBag.SupportsPhysicalInspection = device.DeviceType is DeviceType.Radiologico or DeviceType.Mammografico;
            return View("Create", intervention);
        }

        // ✅ Gestione dei campi opzionali in base al tipo di intervento
        intervention.MaintenanceCategory = intervention.Type == InterventionType.Maintenance
            ? intervention.MaintenanceCategory
            : null;

        intervention.Passed =
            intervention.Type is InterventionType.ElectricalTest or InterventionType.PhysicalInspection
                ? intervention.Passed
                : null;

        // ✅ Salvataggio nel database
        DueDateHelper.UpdateNextDueDate(device, context);
        context.Interventions.Add(intervention);
        await context.SaveChangesAsync();

        // ✅ Messaggio di conferma
        TempData["SuccessMessage"] =
            "Intervento aggiunto con successo! Ora puoi caricare i documenti relativi all’intervento oppure tornare ai dettagli del dispositivo.";

        // ✅ Reindirizza alla pagina di dettagli intervento (dove gestiamo l'upload file)
        return RedirectToAction("Details", "Intervention", new { id = intervention.Id });
    }


    /// <summary>
    ///     Mostra i dettagli di un intervento specifico.
    ///     Accessibile a tutti i ruoli: Admin, Tecnico e Office.
    /// </summary>
    /// <param name="id">ID dell'intervento</param>
    /// <param name="returnUrl">URL di ritorno, utile per tornare alla cronologia o ai dettagli del dispositivo</param>
    /// <returns>La vista dei dettagli dell'intervento</returns>
    [HttpGet("Details/{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico + "," + Roles.Office)]
    public async Task<IActionResult> Details(int id, string? returnUrl = null)
    {
        // ✅ Recupera l'intervento dal database, includendo il dispositivo e gli allegati (file caricati)
        var intervention = await context.Interventions
            .Include(i => i.Device)
            .Include(i => i.Attachments) // Carichiamo direttamente i file associati all'intervento
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null)
            return NotFound("Intervento non trovato.");

        // ✅ Se returnUrl non è stato esplicitamente passato, proviamo a dedurlo dal Referer
        if (string.IsNullOrEmpty(returnUrl))
        {
            var referer = Request.Headers.Referer.ToString();

            if (!string.IsNullOrEmpty(referer))
            {
                // Se siamo arrivati dalla cronologia interventi
                if (referer.Contains("/InterventionHistory/List", StringComparison.OrdinalIgnoreCase))
                    returnUrl = Url.Action("List", "InterventionHistory", new { deviceId = intervention.DeviceId });
                // Se siamo arrivati dai dettagli del dispositivo
                else if (referer.Contains("/Device/Details", StringComparison.OrdinalIgnoreCase))
                    returnUrl = Url.Action("Details", "Device", new { id = intervention.DeviceId });
            }
        }

        // ✅ Se non siamo riusciti a determinare un returnUrl valido, come fallback torniamo ai dettagli dispositivo
        ViewBag.ReturnUrl = returnUrl ?? Url.Action("Details", "Device", new { id = intervention.DeviceId });

        // ✅ Mostriamo la view dei dettagli
        return View("Details", intervention);
    }

    /// <summary>
    ///     Mostra la schermata di modifica di un intervento esistente.
    ///     Accessibile solo a Admin e Tecnico.
    /// </summary>
    /// <param name="id">ID dell'intervento da modificare</param>
    /// <param name="returnUrl">URL di ritorno alla pagina precedente</param>
    [HttpGet("Edit/{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Edit(int id, string? returnUrl = null)
    {
        // ✅ Recupera l'intervento dal database con dispositivo e allegati
        var intervention = await context.Interventions
            .Include(i => i.Attachments)
            .Include(i => i.Device)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null)
            return NotFound("Intervento non trovato.");

        // ✅ Verifica se il dispositivo richiede controlli fisici
        ViewBag.SupportsPhysicalInspection =
            intervention.Device?.DeviceType is DeviceType.Radiologico or DeviceType.Mammografico;

        // ✅ Passa il nome del dispositivo (utile per breadcrumb o intestazioni)
        ViewBag.DeviceName = intervention.Device?.Name ?? "Dispositivo Sconosciuto";

        // ✅ Prepara la URL di ritorno (alla cronologia o ai dettagli del dispositivo)
        ViewBag.ReturnUrl =
            returnUrl ?? Url.Action("List", "InterventionHistory", new { deviceId = intervention.DeviceId });

        return View("Edit", intervention);
    }

    /// <summary>
    ///     Salva le modifiche a un intervento esistente.
    ///     Accessibile solo a Admin e Tecnico.
    /// </summary>
    /// <param name="id">ID dell'intervento modificato</param>
    /// <param name="intervention">Dati aggiornati dell'intervento</param>
    /// <param name="returnUrl">URL di ritorno</param>
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Edit(int id, Intervention intervention, string? returnUrl = null)
    {
        // ✅ Verifica corrispondenza tra URL e dati
        if (id != intervention.Id)
            return BadRequest("L'ID dell'intervento non corrisponde.");

        // ✅ Validazione del modello
        if (!ModelState.IsValid)
            return View("Edit", intervention);

        // ✅ Recupera dal database l'intervento esistente (con il dispositivo associato)
        var existingIntervention = await context.Interventions
            .Include(i => i.Device)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (existingIntervention == null)
            return NotFound("Intervento non trovato.");

        // ✅ Aggiorna i campi modificabili
        existingIntervention.Date = intervention.Date;
        existingIntervention.PerformedBy = intervention.PerformedBy;
        existingIntervention.Notes = intervention.Notes;
        existingIntervention.MaintenanceCategory = intervention.MaintenanceCategory;
        existingIntervention.Type = intervention.Type;
        existingIntervention.Passed = intervention.Passed;

        context.Update(existingIntervention);
        await context.SaveChangesAsync();

        // ✅ Aggiorna le scadenze del dispositivo
        var device = existingIntervention.Device;
        if (device != null)
        {
            DueDateHelper.UpdateNextDueDate(device, context);
            context.Update(device);
            await context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Modifica salvata con successo!";

        // ✅ Torna all'URL di provenienza o a un fallback
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("List", "InterventionHistory", new { deviceId = existingIntervention.DeviceId });
    }

    /// <summary>
    ///     Elimina definitivamente un intervento.
    ///     Questa operazione è riservata esclusivamente agli Admin, in quanto la rimozione di interventi storici deve essere
    ///     limitata
    ///     a figure con massima responsabilità, per preservare la tracciabilità delle attività manutentive.
    /// </summary>
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        // Recupera l'intervento dal database
        var intervention = await context.Interventions.FindAsync(id);
        if (intervention == null)
            return NotFound();

        // Recupera il dispositivo associato, inclusi gli interventi per il ricalcolo delle scadenze
        var device = await context.Devices
            .Include(d => d.Interventions)
            .FirstOrDefaultAsync(d => d.Id == intervention.DeviceId);

        if (device == null)
            return NotFound();

        // Rimuove l'intervento dal database
        context.Interventions.Remove(intervention);
        await context.SaveChangesAsync();

        // Aggiorna le scadenze future del dispositivo in base agli interventi rimasti
        DueDateHelper.UpdateNextDueDate(device, context);

        // Se non ci sono più interventi, resetta le date di manutenzione/verifica ai valori predefiniti
        var settings = await context.MaintenanceSettings.FirstOrDefaultAsync();
        if (settings == null)
            return StatusCode(500, "Errore nel recupero delle impostazioni di manutenzione.");

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

        // Se possibile, ritorna alla pagina precedente (cronologia interventi o dettagli dispositivo)
        var referer = Request.Headers.Referer.ToString();
        if (string.IsNullOrEmpty(returnUrl) && !string.IsNullOrEmpty(referer))
        {
            if (referer.Contains("/InterventionHistory/List", StringComparison.OrdinalIgnoreCase))
                returnUrl = Url.Action("List", "InterventionHistory", new { deviceId = device.Id });
            else if (referer.Contains("/Device/Details", StringComparison.OrdinalIgnoreCase))
                returnUrl = Url.Action("Details", "Device", new { id = device.Id });
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

        // Fallback: Torna ai dettagli del dispositivo
        return RedirectToAction("Details", "Device", new { id = device.Id });
    }
}