using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

public class MaintenanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DeadlineService _deadlineService;
    private readonly FileService _fileService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(ApplicationDbContext context, ILogger<MaintenanceController> logger,
        FileService fileService, DeadlineService deadlineService)
    {
        _context = context;
        _logger = logger;
        _fileService = fileService;
        _deadlineService = deadlineService;
    }

    /// <summary>
    ///     Visualizza l'elenco delle manutenzioni per un determinato dispositivo.
    /// </summary>
    public async Task<IActionResult> Index(int deviceId)
    {
        _logger.LogInformation("Caricamento manutenzioni per il dispositivo {DeviceId}", deviceId);

        var maintenances = await _context.Maintenances
            .Where(m => m.DeviceId == deviceId)
            .Include(m => m.Device)
            .Include(m => m.Documents) // Ora sono FileDocument
            .ToListAsync();

        if (!maintenances.Any())
        {
            _logger.LogWarning("Nessuna manutenzione trovata per il dispositivo {DeviceId}", deviceId);
            TempData["ErrorMessage"] = "Nessuna manutenzione disponibile per questo dispositivo.";
        }

        ViewBag.DeviceId = deviceId;
        return View(maintenances);
    }

    /// <summary>
    ///     Mostra il form per la creazione di una nuova manutenzione.
    /// </summary>
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

    /// <summary>
    ///     Elabora la creazione di una nuova manutenzione.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Maintenance maintenance, List<IFormFile> files)
    {
        _logger.LogInformation("Creazione nuova manutenzione per il dispositivo {DeviceId}", maintenance.DeviceId);

        if (!ModelState.IsValid)
        {
            ViewBag.DeviceId = maintenance.DeviceId;
            return View(maintenance);
        }

        try
        {
            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();

            var device = await _context.Devices.FindAsync(maintenance.DeviceId);
            if (device != null && maintenance.MaintenanceType == "Ordinaria")
            {
                device.LastMaintenanceDate = maintenance.MaintenanceDate;
                _context.Update(device);
                await _context.SaveChangesAsync();
            }

            // Salvataggio file con FileService
            if (files != null && files.Any())
            {
                var savedFiles =
                    await _fileService.SaveFilesAsync(files, maintenance.Id, "Maintenance", device?.Name ?? "Unknown");
                _context.FileDocuments.AddRange(savedFiles);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Manutenzione creata con successo.";
            return RedirectToAction("Index", new { deviceId = maintenance.DeviceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la creazione della manutenzione.");
            ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio.");
            ViewBag.DeviceId = maintenance.DeviceId;
            return View(maintenance);
        }
    }

    /// <summary>
    ///     Elimina una manutenzione e i file associati.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation("Eliminazione della manutenzione con ID: {Id}", id);

        var maintenance = await _context.Maintenances
            .Include(m => m.Documents) // Ora sono FileDocument
            .FirstOrDefaultAsync(m => m.Id == id);

        if (maintenance == null)
        {
            _logger.LogWarning("Manutenzione con ID {Id} non trovata.", id);
            TempData["ErrorMessage"] = "Errore: la manutenzione non è stata trovata.";
            return RedirectToAction("Index");
        }

        // Rimuove i file associati con FileService
        if (maintenance.Documents.Any())
        {
            foreach (var document in maintenance.Documents)
            {
                await _fileService.DeleteFileAsync(document.FilePath);
                _context.FileDocuments.Remove(document);
            }

            await _context.SaveChangesAsync();
        }

        _context.Maintenances.Remove(maintenance);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Manutenzione eliminata con successo.";
        return RedirectToAction("Index", new { deviceId = maintenance.DeviceId });
    }
}