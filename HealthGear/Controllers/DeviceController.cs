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
    ILogger<DeviceController> logger) : Controller
{
    public ILogger<DeviceController> Logger { get; } = logger;

    // 📌 GET: /Device
    // Mostra la lista dei dispositivi attivi e dismessi
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string statusFilter = "attivi")
    {
        var activeDevices = await context.Devices
            .Where(d => d.Status != DeviceStatus.Dismesso)
            .ToListAsync();

        var archivedDevices = await context.Devices
            .Where(d => d.Status == DeviceStatus.Dismesso)
            .ToListAsync();

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
        if (!ModelState.IsValid) return View("Add", device);

        context.Add(device);
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
    public async Task<IActionResult> Edit(int id, Device device)
    {
        if (id != device.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Modify", device);

        context.Update(device);
        await context.SaveChangesAsync();
        await deadlineService.UpdateNextDueDatesAsync(device);

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
        if (device == null)
        {
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return Json(new { success = false, message = "Dispositivo non trovato." });

            return RedirectToAction("Index");
        }

        // ✅ Se il dispositivo è già archiviato, lo riattiviamo
        var wasArchived = device.Status == DeviceStatus.Dismesso;
        device.Status = wasArchived ? DeviceStatus.Attivo : DeviceStatus.Dismesso;

        await context.SaveChangesAsync();

        var successMessage =
            wasArchived ? "Dispositivo riattivato con successo!" : "Dispositivo archiviato con successo!";
        TempData["SuccessMessage"] = successMessage;
        var newStatus = wasArchived ? "Attivo" : "Dismesso";

        // ✅ Se la richiesta è AJAX, ritorniamo il JSON
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return Json(new { success = true, message = successMessage, newStatus });

        // ✅ Se la richiesta è normale, reindirizza alla pagina Dettagli del dispositivo
        return RedirectToAction("Details", new { id });
    }


// 📌 POST: /Device/Delete/{id}
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string confirmName)
    {
        if (string.IsNullOrWhiteSpace(confirmName))
        {
            TempData["ErrorMessage"] = "Richiesta non valida. Devi inserire il nome del dispositivo.";
            return RedirectToAction("Details", new { id });
        }

        var device = await context.Devices.Include(d => d.Interventions).FirstOrDefaultAsync(d => d.Id == id);
        if (device == null)
        {
            TempData["ErrorMessage"] = "Dispositivo non trovato.";
            return RedirectToAction("Index");
        }

        if (device.Interventions.Any())
        {
            TempData["ErrorMessage"] = "Il dispositivo ha interventi registrati e non può essere eliminato.";
            return RedirectToAction("Details", new { id });
        }

        if (!string.Equals(confirmName, device.Name, StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Il nome del dispositivo non corrisponde. Riprova.";
            return RedirectToAction("Details", new { id });
        }

        // ✅ Elimina il dispositivo dal database
        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dispositivo eliminato con successo!";
    
        // ✅ Sempre redirect alla lista dispositivi
        return RedirectToAction("Index");
    }
}