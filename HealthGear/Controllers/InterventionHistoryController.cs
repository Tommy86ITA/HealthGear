#region

using HealthGear.Constants;
using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

#endregion

namespace HealthGear.Controllers;

public class InterventionHistoryController(ApplicationDbContext context, ILogger<InterventionHistoryController> logger)
    : Controller
{
    private const int PageSize = 10;

    /// <summary>
    ///     Lists the interventions for a specific device with optional filters and sorting.
    /// </summary>
    /// <param name="deviceId">The ID of the device.</param>
    /// <param name="searchQuery">Optional search query for intervention notes.</param>
    /// <param name="typeFilter">Optional filter for intervention type.</param>
    /// <param name="passedFilter">Optional filter for intervention outcome (passed).</param>
    /// <param name="dateFrom">Optional start date for filtering interventions.</param>
    /// <param name="dateTo">Optional end date for filtering interventions.</param>
    /// <param name="sortBy">Optional sorting parameter.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <returns>A view with the list of interventions.</returns>
    [HttpGet("Index")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico + "," + Roles.Office)]
    public async Task<IActionResult> List(
        int deviceId,
        string? searchQuery,
        string? typeFilter,
        string? passedFilter,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? sortBy,
        int page = 1)
    {
        if (deviceId <= 0)
            return BadRequest("ID dispositivo non valido.");

        // Recupera il dispositivo (solo i campi necessari per il titolo)
        var device = await context.Devices
            .Where(d => d.Id == deviceId)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Brand,
                d.Model,
                d.SerialNumber,
                d.InventoryNumber
            })
            .FirstOrDefaultAsync();

        if (device == null)
            return NotFound("Dispositivo non trovato.");

        // Inizia la query per gli interventi del dispositivo
        var query = context.Interventions.Where(i => i.DeviceId == device.Id);

        // Filtra per ricerca nelle note (se specificato)
        if (!string.IsNullOrEmpty(searchQuery))
        {
            var pattern = $"%{searchQuery.Trim()}%";
            query = query.Where(i => EF.Functions.Like(i.Notes, pattern));
            logger.LogDebug($"Filtro Note: {searchQuery.Trim()}");
        }

        // Filtra per tipo di intervento
        if (!string.IsNullOrEmpty(typeFilter))
        {
            query = query.Where(i => i.Type.ToString() == typeFilter);
            logger.LogDebug($"Filtro Tipo: {typeFilter}");
        }

        // Filtra per esito (passed)
        if (!string.IsNullOrEmpty(passedFilter) && bool.TryParse(passedFilter, out var passedValue))
        {
            query = query.Where(i => i.Passed == passedValue);
            logger.LogDebug($"Filtro Esito: {passedValue}");
        }

        // Filtra per intervallo di date
        if (dateFrom.HasValue)
        {
            query = query.Where(i => i.Date >= dateFrom.Value);
            logger.LogDebug($"Filtro Data Da: {dateFrom.Value}");
        }

        if (dateTo.HasValue)
        {
            query = query.Where(i => i.Date <= dateTo.Value);
            logger.LogDebug($"Filtro Data A: {dateTo.Value}");
        }

        // Ordinamento: se sortBy non viene passato, ordina per data decrescente
        query = string.IsNullOrEmpty(sortBy)
            ? query.OrderByDescending(i => i.Date)
            : sortBy switch
            {
                "Date" => query.OrderBy(i => i.Date),
                "-Date" => query.OrderByDescending(i => i.Date),
                "Type" => query.OrderBy(i => i.Type),
                "Passed" => query.OrderBy(i => i.Passed),
                _ => query.OrderByDescending(i => i.Date)
            };

        // Calcola il totale degli interventi
        var totalItems = await query.CountAsync();

        // Recupera solo la pagina corrente degli interventi
        var interventionsList = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        logger.LogDebug($"Risultati Totali: {totalItems}");

        // Crea il ViewModel con la lista paginata
        var viewModel = new InterventionHistoryViewModel
        {
            DeviceId = device.Id,
            Interventions = new StaticPagedList<Intervention>(interventionsList, page, PageSize, totalItems)
        };

        // Passa i parametri alla view tramite ViewBag
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
        ViewBag.SortBy = sortBy;
        ViewBag.TypeFilter = typeFilter;
        ViewBag.PassedFilter = passedFilter;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
        ViewBag.SearchQuery = searchQuery;
        ViewBag.DeviceId = device.Id;
        ViewBag.DeviceName = device.Name;
        ViewBag.DeviceBrand = device.Brand;
        ViewBag.DeviceModel = device.Model;
        ViewBag.DeviceSerialNumber = device.SerialNumber;
        ViewBag.DeviceInventoryNumber = device.InventoryNumber;

        // Se la richiesta è AJAX, restituisci la PartialView
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_InterventionHistoryPartial", viewModel);

        return View("List", viewModel);
    }
}