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

public class InterventionHistoryController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 10;

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
            Console.WriteLine($"[DEBUG] Filtro Note: {searchQuery.Trim()}");
        }

        // Filtra per tipo di intervento
        if (!string.IsNullOrEmpty(typeFilter))
        {
            query = query.Where(i => i.Type.ToString() == typeFilter);
            Console.WriteLine($"[DEBUG] Filtro Tipo: {typeFilter}");
        }

        // Filtra per esito (passed)
        if (!string.IsNullOrEmpty(passedFilter) && bool.TryParse(passedFilter, out var passedValue))
        {
            query = query.Where(i => i.Passed == passedValue);
            Console.WriteLine($"[DEBUG] Filtro Esito: {passedValue}");
        }

        // Filtra per intervallo di date
        if (dateFrom.HasValue)
        {
            query = query.Where(i => i.Date >= dateFrom.Value);
            Console.WriteLine($"[DEBUG] Filtro Data Da: {dateFrom.Value}");
        }

        if (dateTo.HasValue)
        {
            query = query.Where(i => i.Date <= dateTo.Value);
            Console.WriteLine($"[DEBUG] Filtro Data A: {dateTo.Value}");
        }

        // Ordinamento: se sortBy non viene passato, ordina per data decrescente
        if (string.IsNullOrEmpty(sortBy))
            query = query.OrderByDescending(i => i.Date);
        else
            query = sortBy switch
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

        Console.WriteLine($"[DEBUG] Risultati Totali: {totalItems}");

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