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

    // GET: ElectricalTest
    public async Task<IActionResult> Index(int deviceId)
    {
        _logger.LogInformation("Caricamento elenco verifiche elettriche per il dispositivo {DeviceId}", deviceId);
        
        var tests = await _context.ElectricalTests
            .Where(e => e.DeviceId == deviceId)
            .Include(e => e.Device)
            .ToListAsync();

        ViewBag.DeviceId = deviceId;
        return View(tests);
    }

    // GET: ElectricalTest/Create
    public IActionResult Create(int deviceId)
    {
        _logger.LogInformation("Accesso alla pagina di creazione per il dispositivo {DeviceId}", deviceId);

        if (deviceId == 0)
        {
            TempData["ErrorMessage"] = "ID dispositivo non valido.";
            return RedirectToAction("Index", "Device");
        }

        ViewBag.DeviceId = deviceId;
        return View();
    }

    // POST: ElectricalTest/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ElectricalTest electricalTest)
    {
        _logger.LogInformation("Ricevuta richiesta di creazione per DeviceId: {DeviceId}", electricalTest.DeviceId);

        if (!ModelState.IsValid)
        {
            foreach (var key in ModelState.Keys)
            {
                foreach (var error in ModelState[key].Errors)
                {
                    _logger.LogWarning("Errore su {Key}: {ErrorMessage}", key, error.ErrorMessage);
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(electricalTest);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Verifica elettrica salvata con successo per DeviceId: {DeviceId}", electricalTest.DeviceId);
                return RedirectToAction(nameof(Index), new { deviceId = electricalTest.DeviceId });
            }
            catch (Exception ex)
            {
                _logger.LogError("Errore durante il salvataggio della verifica: {Message}", ex.Message);
                ModelState.AddModelError("", "Errore durante il salvataggio.");
            }
        }

        ViewBag.DeviceId = electricalTest.DeviceId;
        return View(electricalTest);
    }

    // GET: ElectricalTest/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Accesso alla pagina di modifica per il test ID: {TestId}", id);

        var test = await _context.ElectricalTests.FindAsync(id);
        if (test == null)
        {
            _logger.LogWarning("Verifica elettrica con ID {TestId} non trovata", id);
            return NotFound();
        }

        ViewBag.DeviceId = test.DeviceId;
        return View(test);
    }

    // POST: ElectricalTest/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ElectricalTest electricalTest)
    {
        _logger.LogInformation("Modifica richiesta per ID: {TestId}, DeviceId: {DeviceId}", id, electricalTest.DeviceId);

        if (id != electricalTest.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(electricalTest);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Verifica elettrica aggiornata con successo per DeviceId: {DeviceId}", electricalTest.DeviceId);
                return RedirectToAction(nameof(Index), new { deviceId = electricalTest.DeviceId });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError("Errore di concorrenza: {Message}", ex.Message);
                ModelState.AddModelError("", "Un altro utente ha modificato questo record. Riprova.");
            }
        }

        ViewBag.DeviceId = electricalTest.DeviceId;
        return View(electricalTest);
    }

    // GET: ElectricalTest/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Richiesta di eliminazione per il test ID: {TestId}", id);

        var test = await _context.ElectricalTests.FindAsync(id);
        if (test == null)
        {
            _logger.LogWarning("Verifica elettrica con ID {TestId} non trovata", id);
            return NotFound();
        }

        return View(test);
    }

    // POST: ElectricalTest/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var test = await _context.ElectricalTests.FindAsync(id);
        if (test != null)
        {
            _context.ElectricalTests.Remove(test);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Verifica elettrica con ID {TestId} eliminata", id);
            return RedirectToAction(nameof(Index), new { deviceId = test.DeviceId });
        }

        _logger.LogWarning("Tentativo di eliminazione fallito per ID {TestId}", id);
        TempData["ErrorMessage"] = "Verifica elettrica non trovata.";
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var test = await _context.ElectricalTests
            .FirstOrDefaultAsync(m => m.Id == id);
    
        if (test == null)
        {
            return NotFound();
        }
    
        return View(test);
    }
}