using HealthGear.Data;
using HealthGear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HealthGear.Controllers;

public class MaintenanceController : Controller
{
    private readonly ApplicationDbContext _context;

    public MaintenanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Visualizzazione dell'elenco delle manutenzioni per un dispositivo
    public async Task<IActionResult> Index(int deviceId)
    {
        var maintenances = await _context.Maintenances
            .Where(m => m.DeviceId == deviceId)
            .Include(m => m.Device)
            .Include(m => m.Documents)
            .ToListAsync();

        ViewBag.DeviceId = deviceId;
        return View(maintenances);
    }

    // Visualizzazione del form per creare una nuova manutenzione
    public IActionResult Create(int deviceId)
    {
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
    public async Task<IActionResult> Create(Maintenance maintenance, List<IFormFile> files)
    {
        if (ModelState.IsValid)
        {
            if (maintenance.MaintenanceDate.HasValue)
            {
                DateTime parsedDate = maintenance.MaintenanceDate.Value;

                // Se la manutenzione è ordinaria, aggiorna la scadenza della prossima manutenzione
                if (maintenance.MaintenanceType == "Ordinaria")
                {
                    var device = await _context.Devices.FindAsync(maintenance.DeviceId);
                    if (device != null)
                    {
                        device.DataCollaudo = parsedDate;
                        _context.Update(device);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("MaintenanceDate", "Formato data non valido.");
                return View(maintenance);
            }

            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();

            if (files != null && files.Count > 0)
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

            return RedirectToAction("Index", new { deviceId = maintenance.DeviceId });
        }

        ViewBag.DeviceId = maintenance.DeviceId;
        return View(maintenance);
    }
}