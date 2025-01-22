using HealthGear.Data;
using HealthGear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _context.MaintenanceSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new MaintenanceSettings();
            _context.MaintenanceSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Index(MaintenanceSettings settings)
    {
        if (ModelState.IsValid)
        {
            var existingSettings = await _context.MaintenanceSettings.FirstOrDefaultAsync();
            if (existingSettings != null)
            {
                existingSettings.MaintenanceIntervalMonths = settings.MaintenanceIntervalMonths;
                existingSettings.ElectricalTestIntervalMonths = settings.ElectricalTestIntervalMonths;
                existingSettings.PhysicalInspectionIntervalMonths = settings.PhysicalInspectionIntervalMonths;
                existingSettings.MammographyInspectionIntervalMonths = settings.MammographyInspectionIntervalMonths;
            }
            else
            {
                _context.MaintenanceSettings.Add(settings);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Impostazioni aggiornate con successo!";
            return RedirectToAction("Index");
        }

        return View(settings);
    }
}