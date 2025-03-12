#region

using HealthGear.Constants;
using HealthGear.Data;
using HealthGear.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// <summary>
// Controller per la gestione delle impostazioni di manutenzione.
// </summary>
#endregion

namespace HealthGear.Controllers;

[Authorize(Roles = Roles.Admin)]
public class SettingsController(ApplicationDbContext context) : Controller
{
    /// <summary>
    /// Mostra la pagina delle impostazioni e carica i dati esistenti dal database.
    /// Se non esistono impostazioni, ne crea una nuova.
    /// </summary>
    /// <returns>La vista con le impostazioni di manutenzione.</returns>
    public async Task<IActionResult> Index()
    {
        var settings = await context.MaintenanceSettings.FirstOrDefaultAsync();
        if (settings != null) return View(settings);
        settings = new MaintenanceSettings();
        context.MaintenanceSettings.Add(settings);
        await context.SaveChangesAsync();

        return View(settings);
    }

    /// <summary>
    /// Salva le impostazioni di manutenzione aggiornate.
    /// </summary>
    /// <param name="settings">Le impostazioni di manutenzione aggiornate.</param>
    /// <returns>La vista delle impostazioni o un reindirizzamento alla stessa pagina dopo il salvataggio.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
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