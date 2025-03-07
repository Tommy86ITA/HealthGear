#region

using HealthGear.Data;
using HealthGear.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HealthGear.Controllers;

public class SettingsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var settings = await context.MaintenanceSettings.FirstOrDefaultAsync();
        if (settings != null) return View(settings);
        settings = new MaintenanceSettings();
        context.MaintenanceSettings.Add(settings);
        await context.SaveChangesAsync();

        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Index(MaintenanceSettings settings)
    {
        if (!ModelState.IsValid) return View(settings);
        var existingSettings = await context.MaintenanceSettings.FirstOrDefaultAsync();
        if (existingSettings != null)
        {
            existingSettings.MaintenanceIntervalMonths = settings.MaintenanceIntervalMonths;
            existingSettings.ElectricalTestIntervalMonths = settings.ElectricalTestIntervalMonths;
            existingSettings.PhysicalInspectionIntervalMonths = settings.PhysicalInspectionIntervalMonths;
            existingSettings.MammographyInspectionIntervalMonths = settings.MammographyInspectionIntervalMonths;
        }
        else
        {
            context.MaintenanceSettings.Add(settings);
        }

        await context.SaveChangesAsync();
        TempData["Success"] = "Impostazioni aggiornate con successo!";
        return RedirectToAction("Index");
    }
}