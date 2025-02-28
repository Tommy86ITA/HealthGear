using HealthGear.Data;
using HealthGear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// Se vuoi usare un helper per filtrare

namespace HealthGear.Controllers;

public class HomeController(ApplicationDbContext context) : Controller
{
    // Iniezione del contesto EF

    // Azione principale della Home
    public async Task<IActionResult> Home()
    {
        // 1. Statistiche semplici
        var numDevices = await context.Devices.CountAsync();
        var numInterventions = await context.Interventions.CountAsync();

        // 2. Recupera gli ultimi 5 interventi
        var recentInterventions = await context.Interventions
            .Include(i => i.Device)
            .OrderByDescending(i => i.Date)
            .Take(5)
            .ToListAsync();

        // 3. Recupera i dispositivi con scadenze imminenti o scadute
        // (Se preferisci mostrare solo la prossima manutenzione scaduta, puoi
        //  modificare la logica. Qui mostro un esempio che controlla 3 possibili scadenze.)
        var today = DateTime.Today;
        var threshold = today.AddMonths(2);

        var upcomingDueDates = await context.Devices
            .Where(d =>
                // NextMaintenanceDue scaduta o entro 2 mesi
                (d.NextMaintenanceDue != null && d.NextMaintenanceDue <= threshold) ||
                // NextElectricalTestDue scaduta o entro 2 mesi
                (d.NextElectricalTestDue != null && d.NextElectricalTestDue <= threshold) ||
                // NextPhysicalInspectionDue scaduta o entro 2 mesi
                (d.NextPhysicalInspectionDue != null && d.NextPhysicalInspectionDue <= threshold)
            )
            .OrderBy(d => d.Name)
            .ToListAsync();

        // 4. Popola il modello per la view
        var model = new HomeViewModel
        {
            NumberOfDevices = numDevices,
            NumberOfInterventions = numInterventions,
            RecentInterventions = recentInterventions,
            UpcomingDueDates = upcomingDueDates
        };

        // 5. Restituisce la view "Home.cshtml" in /Views/Home
        return View("Home", model);
    }
}