using HealthGear.Data;
using HealthGear.Models;
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
    /// Mostra la dashboard delle statistiche generali degli interventi.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var interventions = await _context.Interventions
            .Include(i => i.Device) // Assicura che il dispositivo sia caricato
            .ToListAsync();

        // Debug: Verifica se i dati dei dispositivi sono presenti
        foreach (var intervention in interventions)
        {
            if (intervention.Device != null)
            {
                Console.WriteLine($"DEBUG: Device ID: {intervention.Device.Id}, Brand: {intervention.Device.Brand}, Model: {intervention.Device.Model}, Name: {intervention.Device.Name}");
            }
        }

        var totalInterventions = interventions.Count;

        var interventionsByType = interventions
            .GroupBy(i => i.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var correctiveInterventions = interventions
            .Where(i => i.Type == InterventionType.Maintenance && i.MaintenanceCategory == MaintenanceType.Corrective)
            .ToList();

        var topDevices = correctiveInterventions
            .Where(i => i.Device != null)
            .GroupBy(i => new { i.Device!.Id, i.Device!.Name, i.Device!.Brand, i.Device!.Model }) // Raggruppa per ID, Nome, Brand e Model
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new StatisticsViewModel.DeviceCorrectiveMaintenanceStats
            {
                DeviceId = g.Key.Id, // Adesso otteniamo l'ID corretto del dispositivo
                DeviceName = g.Key.Name ?? "Sconosciuto",
                DeviceBrand = g.Key.Brand ?? "Sconosciuto",
                DeviceModel = g.Key.Model ?? "Sconosciuto",
                CorrectiveMaintenanceCount = g.Count()
            })
            .ToList();

        // Debug: Controlla se i dati vengono passati correttamente al ViewModel
        foreach (var device in topDevices)
        {
            Console.WriteLine($"DEBUG: ViewModel - ID: {device.DeviceId}, Brand: {device.DeviceBrand}, Model: {device.DeviceModel}, Name: {device.DeviceName}");
        }

        var recentInterventions = interventions
            .OrderByDescending(i => i.Date)
            .Take(10)
            .Select(i => new StatisticsViewModel.InterventionSummary
            {
                Date = i.Date,
                DeviceName = i.Device?.Name ?? "Sconosciuto",
                Type = i.Type.ToString(),
                Status = i.Passed.HasValue
                    ? i.Passed.Value ? "Superato" : "Non Superato"
                    : "N/D"
            })
            .ToList();

        var viewModel = new StatisticsViewModel(interventions)
        {
            TotalInterventions = totalInterventions,
            InterventionsByType = interventionsByType,
            DevicesWithMostCorrectiveMaintenances = topDevices,
            RecentInterventions = recentInterventions
        };

        return View(viewModel);
    }
}