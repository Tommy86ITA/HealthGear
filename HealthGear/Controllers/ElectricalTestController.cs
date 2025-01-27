using Microsoft.AspNetCore.Mvc;
using HealthGear.Data;
using HealthGear.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthGear.Controllers;

public class ElectricalTestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ElectricalTestController> _logger;

    public ElectricalTestController(ApplicationDbContext context, ILogger<ElectricalTestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Visualizzazione dell'elenco delle verifiche elettriche per un dispositivo
    public async Task<IActionResult> Index(int deviceId)
    {
        _logger.LogInformation("Caricamento verifiche elettriche per il dispositivo {DeviceId}", deviceId);

        var electricalTests = await _context.ElectricalTests
            .Where(e => e.DeviceId == deviceId)
            .Include(e => e.Device)
            .Include(e => e.Documents)
            .ToListAsync();

        if (!electricalTests.Any())
        {
            TempData["ElectricalTestErrorMessage"] = "Nessuna verifica elettrica disponibile.";
        }

        ViewBag.DeviceId = deviceId;
        return View(electricalTests);
    }

    // Visualizzazione del form per creare una nuova verifica elettrica
    public IActionResult Create(int deviceId)
    {
        _logger.LogInformation("Apertura della pagina di creazione per il dispositivo {DeviceId}", deviceId);

        ViewBag.DeviceId = deviceId;
        return View(new ElectricalTest
        {
            DeviceId = deviceId,
            TestDate = DateTime.Today,
            PerformedBy = string.Empty,
            Passed = false
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ElectricalTest electricalTest, List<IFormFile> files)
    {
        _logger.LogInformation("Creazione nuova verifica elettrica per il dispositivo {DeviceId}", electricalTest.DeviceId);

        if (ModelState.IsValid)
        {
            try
            {
                _context.ElectricalTests.Add(electricalTest);
                await _context.SaveChangesAsync();

                // Se la verifica elettrica è stata superata, aggiorniamo la scadenza nel dispositivo
                if (electricalTest.Passed)
                {
                    var device = await _context.Devices.FindAsync(electricalTest.DeviceId);
                    if (device != null)
                    {
                        device.AggiornaProssimaVerificaElettrica(electricalTest.TestDate);
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

                        var document = new ElectricalTestDocument
                        {
                            ElectricalTestId = electricalTest.Id,
                            FileName = file.FileName,
                            FilePath = $"/uploads/{file.FileName}"
                        };

                        _context.ElectricalTestDocuments.Add(document);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "La verifica elettrica è stata aggiunta con successo.";
                return RedirectToAction("Index", new { deviceId = electricalTest.DeviceId });
            }
            catch (Exception ex)
            {
                _logger.LogError("Errore durante la creazione della verifica elettrica: {Message}", ex.Message);
                ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio.");
            }
        }

        ViewBag.DeviceId = electricalTest.DeviceId;
        return View(electricalTest);
    }

    // Visualizzazione dei dettagli di una verifica elettrica
    public async Task<IActionResult> Details(int id)
    {
        _logger.LogInformation("Caricamento dettagli verifica elettrica per ID: {Id}", id);

        var electricalTest = await _context.ElectricalTests
            .Include(e => e.Device)
            .Include(e => e.Documents)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (electricalTest == null)
        {
            _logger.LogWarning("Verifica elettrica con ID {Id} non trovata.", id);
            return NotFound();
        }

        return View(electricalTest);
    }

    // Modifica di una verifica elettrica esistente
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Apertura della pagina di modifica per verifica elettrica ID: {Id}", id);

        var electricalTest = await _context.ElectricalTests.FindAsync(id);

        if (electricalTest == null)
        {
            _logger.LogWarning("Verifica elettrica con ID {Id} non trovata.", id);
            return NotFound();
        }

        return View(electricalTest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ElectricalTest electricalTest, List<IFormFile> files)
    {
        if (id != electricalTest.Id)
        {
            _logger.LogWarning("Tentativo di modifica con ID non corrispondente: {Id}", id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(electricalTest);
                await _context.SaveChangesAsync();

                if (electricalTest.Passed)
                {
                    var device = await _context.Devices.FindAsync(electricalTest.DeviceId);
                    if (device != null)
                    {
                        device.AggiornaProssimaVerificaElettrica(electricalTest.TestDate);
                        _context.Update(device);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "La verifica elettrica è stata aggiornata con successo.";
                return RedirectToAction("Index", new { deviceId = electricalTest.DeviceId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ElectricalTests.Any(e => e.Id == electricalTest.Id))
                {
                    _logger.LogWarning("Verifica elettrica con ID {Id} non trovata durante l'aggiornamento.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        ViewBag.DeviceId = electricalTest.DeviceId;
        return View(electricalTest);
    }

    // Eliminazione di una verifica elettrica
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation("Eliminazione della verifica elettrica con ID: {Id}", id);

        var electricalTest = await _context.ElectricalTests.FindAsync(id);
        if (electricalTest == null)
        {
            TempData["ErrorMessage"] = "Errore: verifica elettrica non trovata.";
            return RedirectToAction("Index");
        }

        _context.ElectricalTests.Remove(electricalTest);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "La verifica elettrica è stata eliminata con successo.";
        return RedirectToAction("Index", new { deviceId = electricalTest.DeviceId });
    }
}