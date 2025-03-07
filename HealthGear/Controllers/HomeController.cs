using HealthGear.Data;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

/// <summary>
///     Controller per la gestione della Home page e della dashboard principale di HealthGear.
/// </summary>
[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ThirdPartyService _thirdPartyService;

    /// <summary>
    ///     Inizializza una nuova istanza del <see cref="HomeController" />.
    /// </summary>
    /// <param name="context">Contesto del database iniettato.</param>
    /// <param name="thirdPartyService">Servizio per la gestione dei componenti di terze parti.</param>
    /// <exception cref="ArgumentNullException">Sollevata se uno dei parametri è null.</exception>
    public HomeController(ApplicationDbContext context, ThirdPartyService thirdPartyService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _thirdPartyService = thirdPartyService ?? throw new ArgumentNullException(nameof(thirdPartyService));
    }

    /// <summary>
    ///     Mostra la dashboard principale di HealthGear.
    ///     Se l'utente non è autenticato, viene reindirizzato alla pagina di login.
    /// </summary>
    /// <returns>La vista della dashboard con i dati riepilogativi.</returns>
    public async Task<IActionResult> Home()
    {
        // Se l'utente non è autenticato, reindirizza alla pagina di login di Identity.
        if (!User.Identity?.IsAuthenticated ?? true) return RedirectToPage("/Account/Login", new { area = "Identity" });

        // Conta dispositivi registrati.
        var numDevices = await _context.Devices.CountAsync();

        // Conta interventi registrati.
        var numInterventions = await _context.Interventions.CountAsync();

        // Recupera i 5 interventi più recenti.
        var recentInterventions = await _context.Interventions
            .Include(i => i.Device)
            .OrderByDescending(i => i.Date)
            .Take(5)
            .ToListAsync();

        // Calcola la soglia di avviso per le scadenze.
        var today = DateTime.Today;
        var threshold = today.AddMonths(2);

        // Trova dispositivi con scadenze imminenti o superate.
        var upcomingDueDates = await _context.Devices
            .Where(d =>
                (d.NextMaintenanceDue != null && d.NextMaintenanceDue <= threshold) ||
                (d.NextElectricalTestDue != null && d.NextElectricalTestDue <= threshold) ||
                (d.NextPhysicalInspectionDue != null && d.NextPhysicalInspectionDue <= threshold)
            )
            .OrderBy(d => d.Name)
            .ToListAsync();

        // Prepara il ViewModel.
        var model = new HomeViewModel
        {
            NumberOfDevices = numDevices,
            NumberOfInterventions = numInterventions,
            RecentInterventions = recentInterventions,
            UpcomingDueDates = upcomingDueDates
        };

        // Restituisce la vista Home.
        return View("Home", model);
    }

    /// <summary>
    ///     Mostra la pagina delle informazioni (About) con i dettagli di versione, build e componenti di terze parti.
    /// </summary>
    /// <returns>La vista About con i dati della versione e i componenti di terze parti.</returns>
    [AllowAnonymous]
    public IActionResult About()
    {
        var model = new AboutViewModel
        {
            // La versione viene recuperata automaticamente da GitVersion nel costruttore di AboutViewModel
            DataBuild = DateTime.Now,
            ThirdPartyComponents = _thirdPartyService.GetThirdPartyComponents()
        };

        return View(model);
    }
}