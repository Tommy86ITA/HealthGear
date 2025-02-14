#region

using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    // Mostra la lista dei dispositivi attivi e dismessi
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string statusFilter = "attivi")
    {
        var activeDevices = await context.Devices
            .Where(d => d.Status != DeviceStatus.Dismesso)
            .ToListAsync();
        ViewBag.CountAttivi = activeDevices.Count;

        var archivedDevices = await context.Devices
            .Where(d => d.Status == DeviceStatus.Dismesso)
            .ToListAsync();
        ViewBag.CountDismessi = archivedDevices.Count;

        var viewModel = new DeviceListViewModel
        {
            ActiveDevices = activeDevices,
            ArchivedDevices = archivedDevices,
            StatusFilter = statusFilter
        };

        return View("List", viewModel);
    }

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

    // 📌 GET: /Device/ConfirmDeleteOrArchive/{id}
    [HttpGet("ConfirmDeleteOrArchive/{id:int}")]
    public async Task<IActionResult> ConfirmDeleteOrArchive(int id)
    {
        var device = await context.Devices.Include(d => d.Interventions).FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        return View("ConfirmDeleteOrArchive", device);
    }

    // 📌 POST: /Device/Archive/{id}
    [HttpPost("Archive/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null) return NotFound();

        // ✅ Alterna lo stato tra Attivo e Dismesso
        var wasArchived = device.Status == DeviceStatus.Dismesso;
        device.Status = wasArchived ? DeviceStatus.Attivo : DeviceStatus.Dismesso;

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = wasArchived ? "Dispositivo riattivato con successo!" : "Dispositivo archiviato con successo!";

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