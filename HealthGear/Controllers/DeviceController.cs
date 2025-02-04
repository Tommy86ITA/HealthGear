#region

using HealthGear.Data;
using HealthGear.Helpers;
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
            return Json(new { success = false, message = "Dispositivo non trovato." });

        if (device.Status == DeviceStatus.Dismesso)
            return Json(new { success = false, message = "Il dispositivo è già archiviato." });

        device.Status = DeviceStatus.Dismesso;
        await context.SaveChangesAsync();

        logger.LogInformation($"Dispositivo {id} archiviato con successo.");
        return Json(new { success = true, message = "Dispositivo archiviato con successo!" });
    }

    // 📌 POST: /Device/Restore/{id}
    [HttpPost("Restore/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null)
            return Json(new { success = false, message = "Dispositivo non trovato." });

        if (device.Status == DeviceStatus.Attivo)
            return Json(new { success = false, message = "Il dispositivo è già attivo." });

        device.Status = DeviceStatus.Attivo;
        await context.SaveChangesAsync();

        logger.LogInformation($"Dispositivo {id} riattivato con successo.");
        return Json(new { success = true, message = "Dispositivo riattivato con successo!" });
    }

    // 📌 POST: /Device/DeleteConfirmed/{id}
    [HttpPost("DeleteConfirmed/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id, [FromBody] ConfirmDeleteModel data)
    {
        if (string.IsNullOrWhiteSpace(data.ConfirmName))
            return BadRequest(new { success = false, message = "Richiesta non valida. Dati mancanti." });

        var device = await context.Devices.Include(d => d.Interventions).FirstOrDefaultAsync(d => d.Id == id);
        if (device == null)
        {
            logger.LogWarning($"Tentata eliminazione: Dispositivo {id} non trovato.");
            return NotFound(new { success = false, message = "Dispositivo non trovato." });
        }

        if (device.Interventions.Any())
        {
            logger.LogWarning($"Eliminazione bloccata: Il dispositivo {id} ha interventi registrati.");
            return BadRequest(new
                { success = false, message = "Il dispositivo ha interventi registrati e non può essere eliminato." });
        }

        if (data.ConfirmName != device.Name)
            return BadRequest(new { success = false, message = "Il nome non corrisponde. Riprova." });

        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        logger.LogInformation($"Dispositivo {id} eliminato con successo.");
        return Json(new { success = true });
    }

    // 📌 GET: /Device/ConfirmDelete/{id}
    [HttpGet("ConfirmDelete/{id:int}")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var device = await context.Devices.Include(d => d.Interventions).FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        if (device.Interventions.Any()) return RedirectToAction("ConfirmDeleteOrArchive", new { id });

        var html = await this.RenderViewToStringAsync("_ConfirmDelete", device);
        return Json(new { success = true, html });
    }
}