#region

using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.EF;

#endregion

namespace HealthGear.Controllers;

[Route("Device")]
public class DeviceController(
    ApplicationDbContext context,
    DeadlineService deadlineService,
    InventoryNumberService inventoryNumberService)
    : Controller

{
    // 📌 GET: /Device
// Mostra la lista dei dispositivi attivi e dismessi con ricerca
[HttpGet("")]
[HttpGet("Index")]
public async Task<IActionResult> Index(
    string statusFilter = "attivi",
    string? searchQuery = null,
    string dueDateFilter = "all",
    int? pageAttivi = 1,
    int? pageDismessi = 1)
{
    const int pageSize = 10;

    var devicesQuery = context.Devices.AsNoTracking().AsQueryable();

    // Filtro per ricerca
    if (!string.IsNullOrEmpty(searchQuery))
    {
        searchQuery = searchQuery.Trim().ToLower();
        devicesQuery = devicesQuery.Where(d => d.Name.ToLower().Contains(searchQuery) ||
                                                d.Brand.ToLower().Contains(searchQuery) ||
                                                d.Model.ToLower().Contains(searchQuery) ||
                                                d.SerialNumber.ToLower().Contains(searchQuery));
    }

    // Filtro per stato delle scadenze (vedi codice precedente)
    var today = DateTime.Today;
    var soonThreshold = today.AddMonths(2);
    if (dueDateFilter == "expired")
    {
        devicesQuery = devicesQuery.Where(d =>
            d.NextMaintenanceDue < today ||
            d.NextElectricalTestDue < today ||
            d.NextPhysicalInspectionDue < today);
    }
    else if (dueDateFilter == "soon")
    {
        devicesQuery = devicesQuery.Where(d =>
            (d.NextMaintenanceDue >= today && d.NextMaintenanceDue < soonThreshold) ||
            (d.NextElectricalTestDue >= today && d.NextElectricalTestDue < soonThreshold) ||
            (d.NextPhysicalInspectionDue >= today && d.NextPhysicalInspectionDue < soonThreshold));
    }
    else if (dueDateFilter == "ok")
    {
        devicesQuery = devicesQuery.Where(d =>
            (d.NextMaintenanceDue >= soonThreshold) &&
            (d.NextElectricalTestDue >= soonThreshold) &&
            (d.NextPhysicalInspectionDue >= soonThreshold));
    }

    // Paginazione separata per dispositivi attivi e dismessi
    var activeDevices = await devicesQuery
        .Where(d => d.Status == DeviceStatus.Attivo)
        .OrderBy(d => d.InventoryNumber)
        .ToPagedListAsync(pageAttivi ?? 1, pageSize);

    var archivedDevices = await devicesQuery
        .Where(d => d.Status == DeviceStatus.Dismesso)
        .OrderBy(d => d.InventoryNumber)
        .ToPagedListAsync(pageDismessi ?? 1, pageSize);

    var viewModel = new DeviceListViewModel
    {
        ActiveDevices = activeDevices,
        ArchivedDevices = archivedDevices,
        StatusFilter = statusFilter
    };

    ViewBag.PageAttivi = pageAttivi;
    ViewBag.PageDismessi = pageDismessi;
    ViewBag.SearchQuery = searchQuery;
    ViewBag.DueDateFilter = dueDateFilter;

    return View("List", viewModel);
}

/*
// 🔹 Metodo helper per filtrare i dispositivi
private static IQueryable<Device> ApplyFilters(
    IQueryable<Device> query, 
    string statusFilter, 
    string? searchQuery, 
    string dueDateFilter)
{
    var today = DateTime.Today;
    var soonThreshold = today.AddMonths(2);

    // 🔹 Filtra per stato attivo/dismesso
    if (statusFilter == "attivi")
    {
        query = query.Where(d => d.Status != DeviceStatus.Dismesso);
    }
    else if (statusFilter == "dismessi")
    {
        query = query.Where(d => d.Status == DeviceStatus.Dismesso);
    }

    // 🔹 Filtra per ricerca
    if (!string.IsNullOrEmpty(searchQuery))
    {
        searchQuery = searchQuery.Trim().ToLower();
        query = query.Where(d => d.Name.ToLower().Contains(searchQuery) ||
                                 d.Brand.ToLower().Contains(searchQuery) ||
                                 d.Model.ToLower().Contains(searchQuery) ||
                                 d.SerialNumber.ToLower().Contains(searchQuery));
    }

    // 🔹 Filtra per stato delle scadenze
    if (dueDateFilter == "expired")
    {
        query = query.Where(d =>
            d.NextMaintenanceDue < today ||
            d.NextElectricalTestDue < today ||
            d.NextPhysicalInspectionDue < today);
    }
    else if (dueDateFilter == "soon")
    {
        query = query.Where(d =>
            (d.NextMaintenanceDue >= today && d.NextMaintenanceDue < soonThreshold) ||
            (d.NextElectricalTestDue >= today && d.NextElectricalTestDue < soonThreshold) ||
            (d.NextPhysicalInspectionDue >= today && d.NextPhysicalInspectionDue < soonThreshold));
    }
    else if (dueDateFilter == "ok")
    {
        query = query.Where(d =>
            (d.NextMaintenanceDue >= soonThreshold) &&
            (d.NextElectricalTestDue >= soonThreshold) &&
            (d.NextPhysicalInspectionDue >= soonThreshold));
    }

    return query;
}
*/

    // 📌 GET: /Device/Details/{id}
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var device = await context.Devices
            .Include(d => d.Interventions)
            .ThenInclude(i => i.Attachments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();
        return View("ViewDetails", device);
    }

    // 📌 GET: /Device/Add
    [HttpGet("Add")]
    public IActionResult Create()
    {
        return View("Add");
    }

    // 📌 POST: /Device/Add
    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Device device)
    {
        ModelState.Remove("InventoryNumber");
        if (!ModelState.IsValid) return View("Add", device);

        // ✅ Generazione automatica del numero di inventario basato sull'anno di collaudo
        device.SetInventoryNumber(await inventoryNumberService.GenerateInventoryNumberAsync(device.DataCollaudo.Year));

        context.Devices.Add(device);
        await context.SaveChangesAsync();
        await deadlineService.UpdateNextDueDatesAsync(device);

        return RedirectToAction(nameof(Index));
    }

    // 📌 GET: /Device/Modify/{id}
    [HttpGet("Modify/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return View("Modify", device);
    }

    // 📌 POST: /Device/Modify/{id}
    [HttpPost("Modify/{id:int}")]
    [ValidateAntiForgeryToken]
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
    public async Task<IActionResult> Delete(int id, string confirmName)
    {
        var device = await context.Devices.Include(d => d.Interventions).FirstOrDefaultAsync(d => d.Id == id);
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
        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dispositivo eliminato con successo!";
        return RedirectToAction("Index");
    }
}