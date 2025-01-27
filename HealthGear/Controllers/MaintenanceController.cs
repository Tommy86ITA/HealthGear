using HealthGear.Data;
using HealthGear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace HealthGear.Controllers;

public class MaintenanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(ApplicationDbContext context, ILogger<MaintenanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Visualizzazione dell'elenco delle manutenzioni per un dispositivo
    public async Task<IActionResult> Index(int deviceId)
    {
        _logger.LogInformation("Caricamento manutenzioni per il dispositivo {DeviceId}", deviceId);

        var maintenances = await _context.Maintenances
            .Where(m => m.DeviceId == deviceId)
            .Include(m => m.Device)
            .Include(m => m.Documents)
            .ToListAsync();

        if (!maintenances.Any())
        {
            _logger.LogWarning("Nessuna manutenzione trovata per il dispositivo {DeviceId}", deviceId);
            TempData["MaintenanceErrorMessage"] = "Nessuna manutenzione disponibile per questo dispositivo.";
        }

        ViewBag.DeviceId = deviceId;
        return View(maintenances);
    }

    // Visualizzazione del form per creare una nuova manutenzione
    public IActionResult Create(int deviceId)
    {
        _logger.LogInformation("Apertura della pagina di creazione per il dispositivo {DeviceId}", deviceId);

        ViewBag.DeviceId = deviceId;
        return View(new Maintenance
        {
            DeviceId = deviceId,
            MaintenanceDate = DateTime.Today,
            Description = string.Empty,
            PerformedBy = string.Empty,
            MaintenanceType = "Ordinaria"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Maintenance maintenance, List<IFormFile> files)
    {
        _logger.LogInformation("Creazione nuova manutenzione per il dispositivo {DeviceId}", maintenance.DeviceId);

        if (ModelState.IsValid)
        {
            try
            {
                _context.Maintenances.Add(maintenance);
                await _context.SaveChangesAsync();

                // Aggiorna la scadenza della prossima manutenzione ordinaria se necessario
                if (maintenance.MaintenanceType == "Ordinaria")
                {
                    var device = await _context.Devices.FindAsync(maintenance.DeviceId);
                    if (device != null)
                    {
                        device.AggiornaProssimaManutenzione(maintenance.MaintenanceDate);
                        _context.Update(device);
                        await _context.SaveChangesAsync();
                    }
                }

                // Gestione degli allegati caricati
                if (files != null && files.Any())
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsPath);

                    foreach (var file in files)
                    {
                        var filePath = Path.Combine(uploadsPath, file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var document = new MaintenanceDocument
                        {
                            MaintenanceId = maintenance.Id,
                            FileName = file.FileName,
                            FilePath = $"/uploads/{file.FileName}"
                        };

                        _context.MaintenanceDocuments.Add(document);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Manutenzione creata con successo.";
                return RedirectToAction("Index", new { deviceId = maintenance.DeviceId });
            }
            catch (Exception ex)
            {
                _logger.LogError("Errore durante la creazione della manutenzione: {Message}", ex.Message);
                ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio.");
            }
        }

        ViewBag.DeviceId = maintenance.DeviceId;
        return View(maintenance);
    }

    // Visualizzazione dei dettagli di una manutenzione
    public async Task<IActionResult> Details(int id)
    {
        _logger.LogInformation("Caricamento dettagli manutenzione per ID: {Id}", id);

        var maintenance = await _context.Maintenances
            .Include(m => m.Device)
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (maintenance == null)
        {
            _logger.LogWarning("Manutenzione con ID {Id} non trovata.", id);
            return NotFound();
        }

        return View(maintenance);
    }

    // Visualizzazione della pagina di modifica della manutenzione
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Apertura della pagina di modifica per manutenzione ID: {Id}", id);

        var maintenance = await _context.Maintenances.FindAsync(id);
        if (maintenance == null)
        {
            _logger.LogWarning("Manutenzione con ID {Id} non trovata.", id);
            return NotFound();
        }

        return View(maintenance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Maintenance maintenance, List<IFormFile> files)
    {
        if (id != maintenance.Id)
        {
            _logger.LogWarning("Tentativo di modifica con ID non corrispondente: {Id}", id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(maintenance);
                await _context.SaveChangesAsync();

                if (maintenance.MaintenanceType == "Ordinaria")
                {
                    var device = await _context.Devices.FindAsync(maintenance.DeviceId);
                    if (device != null)
                    {
                        device.AggiornaProssimaManutenzione(maintenance.MaintenanceDate);
                        _context.Update(device);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Manutenzione aggiornata con successo.";
                return RedirectToAction("Index", new { deviceId = maintenance.DeviceId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Maintenances.Any(m => m.Id == maintenance.Id))
                {
                    _logger.LogWarning("Manutenzione con ID {Id} non trovata durante l'aggiornamento.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        ViewBag.DeviceId = maintenance.DeviceId;
        return View(maintenance);
    }

    // Eliminazione della manutenzione
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation("Eliminazione della manutenzione con ID: {Id}", id);

        var maintenance = await _context.Maintenances.FindAsync(id);
        if (maintenance == null)
        {
            _logger.LogWarning("Manutenzione con ID {Id} non trovata.", id);
            TempData["MaintenanceErrorMessage"] = "Errore: la manutenzione non è stata trovata.";
            return RedirectToAction("Index");
        }

        _context.Maintenances.Remove(maintenance);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Manutenzione eliminata con successo.";
        return RedirectToAction("Index", new { deviceId = maintenance.DeviceId });
    }
}