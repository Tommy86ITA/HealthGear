using HealthGear.Data;
using HealthGear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

public class DeviceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(ApplicationDbContext context, ILogger<DeviceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Visualizza l'elenco dei dispositivi
    public async Task<IActionResult> Index()
    {
        var devices = await _context.Devices.ToListAsync();

        if (!devices.Any())
        {
            _logger.LogWarning("Nessun dispositivo trovato.");
            TempData["DeviceErrorMessage"] = "Nessun dispositivo disponibile.";
        }

        return View(devices);
    }

    // Visualizza il form per aggiungere un dispositivo
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Device device)
    {
        _logger.LogInformation("Chiamato metodo Create per dispositivo: {SerialNumber}", device.SerialNumber);

        if (await _context.Devices.AnyAsync(d => d.SerialNumber == device.SerialNumber))
        {
            ModelState.AddModelError("SerialNumber", "Esiste già un dispositivo con questo numero di serie.");
            return View(device);
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState non valido per il dispositivo: {@Device}", device);
            return View(device);
        }

        try
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Dispositivo aggiunto con successo!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio del dispositivo: {SerialNumber}", device.SerialNumber);
            ModelState.AddModelError("", "Si è verificato un errore. Riprova più tardi.");
            return View(device);
        }
    }

    // Visualizza il form per modificare un dispositivo
    public async Task<IActionResult> Edit(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return View(device);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Device device)
    {
        if (id != device.Id) return NotFound();

        if (await _context.Devices.AnyAsync(d => d.SerialNumber == device.SerialNumber && d.Id != device.Id))
        {
            ModelState.AddModelError("SerialNumber", "Esiste già un dispositivo con questo numero di serie.");
            return View(device);
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState non valido per il dispositivo: {@Device}", device);
            return View(device);
        }

        try
        {
            _context.Devices.Update(device);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Modifiche salvate con successo!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio delle modifiche al dispositivo: {SerialNumber}",
                device.SerialNumber);
            ModelState.AddModelError("", "Si è verificato un errore. Riprova più tardi.");
            return View(device);
        }
    }

    // Visualizza i dettagli di un dispositivo
    public async Task<IActionResult> Details(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return View(device);
    }

    // Conferma la rimozione di un dispositivo
    public async Task<IActionResult> Delete(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return View(device);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device != null)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Dispositivo eliminato con successo!";
        }

        return RedirectToAction(nameof(Index));
    }
}