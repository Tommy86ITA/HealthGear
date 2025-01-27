using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthGear.Data;
using HealthGear.Models;
using Microsoft.Extensions.Logging;

namespace HealthGear.Controllers;

public class PhysicalInspectionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PhysicalInspectionController> _logger;

    public PhysicalInspectionController(ApplicationDbContext context, ILogger<PhysicalInspectionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: PhysicalInspection/Index
    public async Task<IActionResult> Index(int deviceId)
    {
        _logger.LogInformation("Caricamento controlli fisici per il dispositivo {DeviceId}", deviceId);

        var inspections = await _context.PhysicalInspections
            .Where(p => p.DeviceId == deviceId)
            .Include(p => p.Device)
            .Include(p => p.Documents)
            .ToListAsync();

        if (!inspections.Any())
        {
            TempData["PhysicalInspectionErrorMessage"] = "Nessun controllo fisico disponibile per questo dispositivo.";
        }

        ViewBag.DeviceId = deviceId;
        return View(inspections);
    }

    // GET: PhysicalInspection/Create
    public IActionResult Create(int deviceId)
    {
        _logger.LogInformation("Apertura della pagina di creazione per il dispositivo {DeviceId}", deviceId);

        ViewBag.DeviceId = deviceId;
        return View(new PhysicalInspection
        {
            DeviceId = deviceId,
            InspectionDate = DateTime.Today,
            PerformedBy = string.Empty,
            Notes = string.Empty
        });
    }

    // POST: PhysicalInspection/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PhysicalInspection physicalInspection, List<IFormFile> files)
    {
        _logger.LogInformation("Creazione nuovo controllo fisico per il dispositivo {DeviceId}", physicalInspection.DeviceId);

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(physicalInspection);
                await _context.SaveChangesAsync();

                // Aggiorna la scadenza del controllo fisico
                var device = await _context.Devices.FindAsync(physicalInspection.DeviceId);
                if (device != null)
                {
                    device.AggiornaProssimoControlloFisico(physicalInspection.InspectionDate);
                    _context.Update(device);
                    await _context.SaveChangesAsync();
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

                        var document = new PhysicalInspectionDocument
                        {
                            PhysicalInspectionId = physicalInspection.Id,
                            FileName = file.FileName,
                            FilePath = $"/uploads/{file.FileName}"
                        };

                        _context.PhysicalInspectionDocuments.Add(document);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Il controllo fisico è stato aggiunto con successo.";
                return RedirectToAction(nameof(Index), new { deviceId = physicalInspection.DeviceId });
            }
            catch (Exception ex)
            {
                _logger.LogError("Errore durante la creazione del controllo fisico: {Message}", ex.Message);
                ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio.");
            }
        }

        ViewBag.DeviceId = physicalInspection.DeviceId;
        return View(physicalInspection);
    }

    // GET: PhysicalInspection/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("Apertura della pagina di modifica per controllo fisico ID: {Id}", id);

        var inspection = await _context.PhysicalInspections.FindAsync(id);
        if (inspection == null)
        {
            _logger.LogWarning("Controllo fisico con ID {Id} non trovato.", id);
            TempData["ErrorMessage"] = "Errore: il controllo fisico non è stato trovato.";
            return RedirectToAction(nameof(Index));
        }

        return View(inspection);
    }

    // POST: PhysicalInspection/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PhysicalInspection physicalInspection)
    {
        if (id != physicalInspection.Id)
        {
            _logger.LogWarning("Tentativo di modifica con ID non corrispondente: {Id}", id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(physicalInspection);
                await _context.SaveChangesAsync();

                // Aggiorna la scadenza del controllo fisico
                var device = await _context.Devices.FindAsync(physicalInspection.DeviceId);
                if (device != null)
                {
                    device.AggiornaProssimoControlloFisico(physicalInspection.InspectionDate);
                    _context.Update(device);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Il controllo fisico è stato aggiornato con successo.";
                return RedirectToAction(nameof(Index), new { deviceId = physicalInspection.DeviceId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PhysicalInspections.Any(e => e.Id == physicalInspection.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        return View(physicalInspection);
    }

    // POST: PhysicalInspection/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Eliminazione del controllo fisico con ID: {Id}", id);

        var inspection = await _context.PhysicalInspections.FindAsync(id);
        if (inspection == null)
        {
            _logger.LogWarning("Controllo fisico con ID {Id} non trovato.", id);
            TempData["ErrorMessage"] = "Errore: il controllo fisico non è stato trovato.";
            return RedirectToAction(nameof(Index));
        }

        _context.PhysicalInspections.Remove(inspection);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Il controllo fisico è stato eliminato con successo.";
        return RedirectToAction(nameof(Index), new { deviceId = inspection.DeviceId });
    }
}