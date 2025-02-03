using HealthGear.Data;
using HealthGear.Helpers;
using HealthGear.Models;
using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

[Route("Device")]
public class DeviceController(ApplicationDbContext context, DeadlineService deadlineService) : Controller
{
    // GET: /Device
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var devices = await context.Devices.Include(d => d.Interventions).ToListAsync();
        return View("List", devices);
    }

    // GET: /Device/Details/{id}
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var device = await context.Devices
            .Include(d => d.Interventions) // 🔥 Carica gli interventi
            .ThenInclude(i => i.Attachments) // 🔥 Carica anche gli allegati, se presenti
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();
        return View("ViewDetails", device);
    }

    // GET: /Device/Add
    [HttpGet("Add")]
    public IActionResult Create()
    {
        return View("Add");
    }

    // POST: /Device/Add
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

    // GET: /Device/Modify/{id}
    [HttpGet("Modify/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return View("Modify", device);
    }

    // POST: /Device/Modify/{id}
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

    // GET: /Device/ConfirmDeleteOrArchive/{id}
    [HttpGet("ConfirmDeleteOrArchive/{id:int}")]
    public async Task<IActionResult> ConfirmDeleteOrArchive(int id)
    {
        var device = await context.Devices.Include(d => d.Interventions).FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        return View("ConfirmDeleteOrArchive", device);
    }

    // POST: /Device/ArchiveDevice/{id}
    [HttpPost("ArchiveDevice/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveDevice(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null) return NotFound();

        device.Status = DeviceStatus.Dismesso;
        await context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

// POST: /Device/DeleteConfirmed/{id}
    [HttpPost("DeleteConfirmed/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id, [FromBody] ConfirmDeleteModel data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.ConfirmName))
            return BadRequest(new { success = false, message = "Richiesta non valida. Dati mancanti." });

        var device = await context.Devices.FindAsync(id);
        if (device == null) return NotFound(new { success = false, message = "Dispositivo non trovato." });

        if (data.ConfirmName != device.Name)
            return BadRequest(new { success = false, message = "Il nome non corrisponde. Riprova." });

        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        return Json(new { success = true });
    }

    // GET: /Device/ConfirmDelete/{id}
    [HttpGet("ConfirmDelete/{id:int}")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var device = await context.Devices.Include(d => d.Interventions).FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        if (device.Interventions.Any())
            // Se ha interventi, reindirizza alla pagina di conferma completa
            return RedirectToAction("ConfirmDeleteOrArchive", new { id });

        // Se non ha interventi, mostra solo il modal di conferma
        var html = await this.RenderViewToStringAsync("_ConfirmDelete", device);
        return Json(new { success = true, html });
    }
}