using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

public class ElectricalTestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly FileService _fileService;
    private readonly ILogger<ElectricalTestController> _logger;

    public ElectricalTestController(ApplicationDbContext context, ILogger<ElectricalTestController> logger, FileService fileService)
    {
        _context = context;
        _logger = logger;
        _fileService = fileService;
    }

    /// <summary>
    /// Visualizza l'elenco delle verifiche elettriche per un dispositivo.
    /// </summary>
    public async Task<IActionResult> Index(int deviceId)
    {
        _logger.LogInformation("Caricamento verifiche elettriche per il dispositivo {DeviceId}", deviceId);

        var tests = await _context.ElectricalTests
            .Where(t => t.DeviceId == deviceId)
            .Include(t => t.Device)
            .Include(t => t.Documents)
            .ToListAsync();

        if (!tests.Any())
        {
            _logger.LogWarning("Nessuna verifica elettrica trovata per il dispositivo {DeviceId}", deviceId);
            TempData["ErrorMessage"] = "Nessuna verifica elettrica disponibile per questo dispositivo.";
        }

        ViewBag.DeviceId = deviceId;
        return View(tests);
    }

    /// <summary>
    /// Mostra il form per la creazione di una nuova verifica elettrica.
    /// </summary>
    public IActionResult Create(int deviceId)
    {
        _logger.LogInformation("Apertura della pagina di creazione per il dispositivo {DeviceId}", deviceId);

        ViewBag.DeviceId = deviceId;
        return View(new ElectricalTest
        {
            DeviceId = deviceId,
            TestDate = DateTime.Today,
            PerformedBy = string.Empty,
            Passed = false // Imposta il valore predefinito come "Non superato"
        });
    }

    /// <summary>
    /// Elabora la creazione di una nuova verifica elettrica.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ElectricalTest test, List<IFormFile> files)
    {
        _logger.LogInformation("Creazione nuova verifica elettrica per il dispositivo {DeviceId}", test.DeviceId);

        if (!ModelState.IsValid)
        {
            ViewBag.DeviceId = test.DeviceId;
            return View(test);
        }

        try
        {
            _context.ElectricalTests.Add(test);
            await _context.SaveChangesAsync();

            // Salvataggio dei file tramite FileService
            if (files != null && files.Any())
            {
                var savedFiles = await _fileService.SaveFilesAsync(files, test.Id, "ElectricalTest", test.Device?.Name ?? "Unknown");
                _context.FileDocuments.AddRange(savedFiles);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Verifica elettrica creata con successo.";
            return RedirectToAction("Index", new { deviceId = test.DeviceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la creazione della verifica elettrica.");
            ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio.");
            ViewBag.DeviceId = test.DeviceId;
            return View(test);
        }
    }

    /// <summary>
    /// Mostra il form per la modifica di una verifica elettrica esistente.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Apertura della pagina di modifica per verifica elettrica ID: {Id}", id);

        var test = await _context.ElectricalTests
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (test == null)
        {
            _logger.LogWarning("Verifica elettrica con ID {Id} non trovata.", id);
            return NotFound();
        }

        return View(test);
    }

    /// <summary>
    /// Elabora la modifica di una verifica elettrica esistente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ElectricalTest test, List<IFormFile> files)
    {
        if (id != test.Id)
        {
            _logger.LogWarning("Tentativo di modifica con ID non corrispondente: {Id}", id);
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.DeviceId = test.DeviceId;
            return View(test);
        }

        try
        {
            _context.Update(test);
            await _context.SaveChangesAsync();

            // Salvataggio dei nuovi file tramite FileService
            if (files != null && files.Any())
            {
                var savedFiles = await _fileService.SaveFilesAsync(files, test.Id, "ElectricalTest", test.Device?.Name ?? "Unknown");
                _context.FileDocuments.AddRange(savedFiles);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Verifica elettrica aggiornata con successo.";
            return RedirectToAction("Index", new { deviceId = test.DeviceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la modifica della verifica elettrica.");
            ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio.");
            ViewBag.DeviceId = test.DeviceId;
            return View(test);
        }
    }

    /// <summary>
    /// Visualizza i dettagli di una verifica elettrica.
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0) return NotFound("ID non valido.");

        var test = await _context.ElectricalTests
            .Include(t => t.Device)
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (test == null)
        {
            TempData["ErrorMessage"] = "La verifica elettrica richiesta non è stata trovata.";
            return RedirectToAction("Index");
        }

        return View(test);
    }

    /// <summary>
    /// Elimina una verifica elettrica.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation("Eliminazione della verifica elettrica con ID: {Id}", id);

        var test = await _context.ElectricalTests
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (test == null)
        {
            _logger.LogWarning("Verifica elettrica con ID {Id} non trovata.", id);
            TempData["ErrorMessage"] = "Errore: la verifica elettrica non è stata trovata.";
            return RedirectToAction("Index");
        }

        // Rimuove eventuali documenti associati tramite FileService
        if (test.Documents.Any())
        {
            foreach (var document in test.Documents)
            {
                await _fileService.DeleteFileAsync(document.FilePath);
                _context.FileDocuments.Remove(document);
            }
        }

        _context.ElectricalTests.Remove(test);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Verifica elettrica eliminata con successo.";
        return RedirectToAction("Index", new { deviceId = test.DeviceId });
    }
}