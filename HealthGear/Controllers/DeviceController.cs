#region

using HealthGear.Constants;
using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.EF;

#endregion

namespace HealthGear.Controllers;

[Route("Device")]
public class DeviceController(
    ApplicationDbContext context,
    DeadlineService deadlineService,
    InventoryNumberService inventoryNumberService,
    IWebHostEnvironment env)
    : Controller

{
    // 📌 GET: /Device
    // Mostra la lista dei dispositivi attivi e dismessi con ricerca
    [HttpGet("")]
    [HttpGet("Index")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico + "," + Roles.Office)]
    public async Task<IActionResult> Index(
        string statusFilter = "attivi",
        string? searchQuery = null,
        string dueDateFilter = "all",
        int? pageAttivi = 1,
        int? pageDismessi = 1)
    {
        const int pageSize = 10;

        // Recupera la query di base (tutti i dispositivi)
        var devicesQuery = context.Devices.AsNoTracking().AsQueryable();

        // Filtro per ricerca
        if (!string.IsNullOrEmpty(searchQuery))
        {
            searchQuery = searchQuery.Trim().ToLower();
            devicesQuery = devicesQuery.Where(d =>
                d.Name.ToLower().Contains(searchQuery) ||
                d.Brand.ToLower().Contains(searchQuery) ||
                d.Model.ToLower().Contains(searchQuery) ||
                d.SerialNumber.ToLower().Contains(searchQuery));
        }

        // Filtro per stato delle scadenze
        var today = DateTime.Today;
        var soonThreshold = today.AddMonths(2);
        devicesQuery = dueDateFilter switch
        {
            "expired" => devicesQuery.Where(d =>
                d.NextMaintenanceDue < today || d.NextElectricalTestDue < today || d.NextPhysicalInspectionDue < today),
            "soon" => devicesQuery.Where(d =>
                (d.NextMaintenanceDue >= today && d.NextMaintenanceDue < soonThreshold) ||
                (d.NextElectricalTestDue >= today && d.NextElectricalTestDue < soonThreshold) ||
                (d.NextPhysicalInspectionDue >= today && d.NextPhysicalInspectionDue < soonThreshold)),
            "ok" => devicesQuery.Where(d =>
                d.NextMaintenanceDue >= soonThreshold && d.NextElectricalTestDue >= soonThreshold &&
                d.NextPhysicalInspectionDue >= soonThreshold),
            _ => devicesQuery
        };

        // Applica la paginazione separata per dispositivi attivi e dismessi
        var activeDevices = await devicesQuery
            .Where(d => d.Status == DeviceStatus.Attivo)
            .OrderBy(d => d.InventoryNumber)
            .ToPagedListAsync(pageAttivi ?? 1, pageSize);

        var archivedDevices = await devicesQuery
            .Where(d => d.Status == DeviceStatus.Dismesso)
            .OrderBy(d => d.InventoryNumber)
            .ToPagedListAsync(pageDismessi ?? 1, pageSize);

        // Crea il ViewModel
        var viewModel = new DeviceListViewModel
        {
            ActiveDevices = activeDevices,
            ArchivedDevices = archivedDevices,
            StatusFilter = statusFilter
        };

        // Passiamo i parametri alla view (o partial) tramite ViewBag
        ViewBag.PageAttivi = pageAttivi;
        ViewBag.PageDismessi = pageDismessi;
        ViewBag.SearchQuery = searchQuery;
        ViewBag.DueDateFilter = dueDateFilter;

        // 🔑 Controlla se la richiesta è AJAX
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            // Se è una richiesta AJAX, restituiamo la partial view (senza layout)
            return PartialView("_DeviceListPartial", viewModel);

        // Altrimenti, restituiamo la view completa
        return View("List", viewModel);
    }

    // 📌 GET: /Device/Details/{id}
    [HttpGet("Details/{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico + "," + Roles.Office)]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var device = await context.Devices
            .Include(d => d.FileAttachments) // Includi i file direttamente allegati al dispositivo
            .Include(d => d.Interventions)
            .ThenInclude(i => i.Attachments) // Includi i file allegati agli interventi
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
            return NotFound();

        if (User.Identity?.IsAuthenticated ?? false) return View("ViewDetails", device);
        return View("PublicDetails", device);
    }

    // GET: /Device/Add
    [HttpGet("Add")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public IActionResult Create()
    {
        // Restituisce la view "Add" per la creazione del dispositivo
        return View("Add");
    }

// POST: /Device/Add
    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Create(Device device)
    {
        // Rimuove "InventoryNumber" dalla validazione, poiché verrà generato automaticamente
        ModelState.Remove("InventoryNumber");
        if (!ModelState.IsValid)
            return View("Add", device);

        // Genera automaticamente il numero di inventario in base all'anno di collaudo
        device.SetInventoryNumber(await inventoryNumberService.GenerateInventoryNumberAsync(device.DataCollaudo.Year));

        // Aggiunge il dispositivo al contesto e salva le modifiche
        context.Devices.Add(device);
        await context.SaveChangesAsync();

        // Aggiorna le prossime scadenze (se necessario)
        await deadlineService.UpdateNextDueDatesAsync(device);

        // Imposta un messaggio di successo (facoltativo)
        TempData["SuccessMessage"] =
            "Dispositivo creato con successo! Ora puoi caricare i documenti relativi al dispositivo.";

        // Reindirizza alla pagina dei dettagli del dispositivo appena creato,
        // dove l'utente potrà caricare i file tramite la partial
        return RedirectToAction("Details", "Device", new { id = device.Id });
    }

    // 📌 GET: /Device/Modify/{id}
    [HttpGet("Modify/{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Edit(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return View("Modify", device);
    }

    // 📌 POST: /Device/Modify/{id}
    [HttpPost("Modify/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Edit(int id, Device updatedDevice)
    {
        if (id != updatedDevice.Id) return BadRequest();

        // 🔹 Rimuoviamo InventoryNumber e Status dalla validazione
        ModelState.Remove("InventoryNumber");
        ModelState.Remove("Status");
        if (!ModelState.IsValid) return View("Modify", updatedDevice);

        var existingDevice = await context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (existingDevice == null) return NotFound();

        // ✅ Manteniamo invariato il numero di inventario
        updatedDevice.SetInventoryNumber(existingDevice.InventoryNumber);
        updatedDevice.Status = existingDevice.Status;

        context.Entry(updatedDevice).State = EntityState.Modified;
        await context.SaveChangesAsync();
        await deadlineService.UpdateNextDueDatesAsync(updatedDevice);

        return RedirectToAction(nameof(Index));
    }

    // 📌 POST: /Device/Archive/{id}
    [HttpPost("Archive/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Archive(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null) return NotFound();

        // Se il dispositivo è attivo, stiamo per dismetterlo
        if (device.Status != DeviceStatus.Dismesso)
        {
            // Qui potresti inserire la logica per chiedere conferma all'utente (ad esempio, tramite un modal nella view)
            // Se la conferma arriva, impostiamo lo stato su Dismesso e registriamo la Data di Dismissione.
            device.Status = DeviceStatus.Dismesso;
            device.DataDismissione = DateTime.Now;
            // Opzionalmente, puoi decidere di non aggiornare le scadenze oppure di cancellarle, ad es.:
            // device.NextMaintenanceDue = null;
            // device.NextElectricalTestDue = null;
            // device.NextPhysicalInspectionDue = null;
        }
        else
        {
            // Se il dispositivo è dismesso, lo riattiviamo
            device.Status = DeviceStatus.Attivo;
            device.DataDismissione = null; // oppure lascia la data come riferimento storico
            // Ricalcola le scadenze per il dispositivo attivo
            await deadlineService.UpdateNextDueDatesAsync(device);
        }

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            device.Status == DeviceStatus.Dismesso
                ? "Dispositivo dismesso con successo!"
                : "Dispositivo riattivato con successo!";

        return RedirectToAction("Details", new { id });
    }

    // 📌 POST: /Device/Delete/{id}
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, string confirmName)
    {
        var device = await context.Devices.Include(d => d.Interventions).Include(device => device.FileAttachments)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        if (device.Interventions.Count > 0)
        {
            TempData["ErrorMessage"] = "Il dispositivo ha interventi registrati e non può essere eliminato.";
            return RedirectToAction("Details", new { id });
        }

        if (!string.Equals(confirmName, device.Name, StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Il nome del dispositivo non corrisponde. Riprova.";
            return RedirectToAction("Details", new { id });
        }

        // ✅ Eliminazione definitiva del dispositivo
        // Rimuovi i record dei file dal database

        foreach (var attachment in device.FileAttachments)
        {
            var filePath = Path.Combine(env.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                try
                {
                    System.IO.File.Delete(filePath);
                    Console.WriteLine($"File eliminato: {filePath}");
                }
                catch (Exception ex)
                {
                    // Log dell'errore; potresti anche decidere di interrompere l'operazione se è critico
                    await Console.Error.WriteLineAsync($"Errore eliminando file {attachment.FileName}: {ex.Message}");
                }
            else
                Console.WriteLine($"File non trovato: {filePath}");
        }

        context.FileAttachments.RemoveRange(device.FileAttachments);
        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dispositivo eliminato con successo!";
        return RedirectToAction("Index");
    }

    // 📌 GET: /Device/GenerateQr/{id}
    // Genera il QR Code per il dispositivo con l'ID specificato
    [HttpGet("GenerateQr/{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico + "," + Roles.Office)]
    public IActionResult GenerateQr(int id, [FromServices] QrCodeService qrCodeService)
    {
        var device = context.Devices.Find(id);
        if (device == null) return NotFound();

        var qrCodeImage = qrCodeService.GenerateQrCode(id);
        return File(qrCodeImage, "image/png");
    }
}