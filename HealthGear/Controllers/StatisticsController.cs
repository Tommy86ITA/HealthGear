using HealthGear.Data;
using HealthGear.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Mostra la dashboard delle statistiche generali degli interventi
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var totalInterventions = await _context.Interventions.CountAsync();

        var interventionsByType = await _context.Interventions
            .GroupBy(i => i.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() }) // Conversione esplicita di enum
            .ToDictionaryAsync(g => g.Type, g => g.Count);

        var topDevices = await _context.Interventions
            .Where(i => i.Device != null) // Assicura che Device non sia null
            .GroupBy(i => i.Device!.Name)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new DeviceInterventionCount
            {
                DeviceName = g.Key, // Se null, usa valore predefinito
                InterventionCount = g.Count()
            })
            .ToListAsync();

        var recentInterventions = await _context.Interventions
            .OrderByDescending(i => i.Date)
            .Take(10)
            .Select(i => new InterventionSummary
            {
                Date = i.Date,
                DeviceName = i.Device != null ? i.Device.Name : "Sconosciuto", // Gestione esplicita di null
                Type = i.Type.ToString(), // Conversione enum in stringa
                Status = i.Passed.HasValue
                    ? i.Passed.Value ? "Superato" : "Non Superato"
                    : "N/D" // Gestione bool nullable
            })
            .ToListAsync();

        var interventions = await _context.Interventions.ToListAsync();

        var viewModel = new StatisticsViewModel(interventions)
        {
            TotalInterventions = totalInterventions,
            InterventionsByType = interventionsByType,
            TopDevicesByInterventions = topDevices,
            RecentInterventions = recentInterventions
        };

        return View(viewModel);
    }
}