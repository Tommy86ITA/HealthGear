using HealthGear.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

public class InterventionHistoryController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> List(int deviceId, string typeFilter, string passedFilter, DateTime? dateFrom,
        DateTime? dateTo, string sortBy, int page = 1)
    {
        // ✅ Debug: Stampiamo i parametri ricevuti nella console
        Console.WriteLine(
            $"[DEBUG] DeviceId: {deviceId}, TypeFilter: {typeFilter}, PassedFilter: {passedFilter}, DateFrom: {dateFrom}, DateTo: {dateTo}, SortBy: {sortBy}, Page: {page}");

        // ✅ Controlliamo se il dispositivo esiste
        var device = await context.Devices
            .Where(d => d.Id == deviceId)
            .Select(d => new { d.Id, d.Name })
            .FirstOrDefaultAsync();

        if (device == null) return NotFound("Dispositivo non trovato.");

        // 🔍 FILTRI - Ottimizziamo la query LINQ direttamente sul database
        var query = context.Interventions
            .Where(i => i.DeviceId == device.Id);

        if (!string.IsNullOrEmpty(typeFilter))
        {
            query = query.Where(i => i.Type.ToString() == typeFilter);
            Console.WriteLine($"[DEBUG] Filtro Tipo: {typeFilter}");
        }

        if (bool.TryParse(passedFilter, out var passedValue))
        {
            query = query.Where(i => i.Passed == passedValue);
            Console.WriteLine($"[DEBUG] Filtro Esito: {passedValue}");
        }

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

        // 🔀 ORDINAMENTO
        query = sortBy switch
        {
            "Type" => query.OrderBy(i => i.Type),
            "Passed" => query.OrderBy(i => i.Passed),
            _ => query.OrderByDescending(i => i.Date)
        };

        // 📄 PAGINAZIONE
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        var interventions = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        Console.WriteLine($"[DEBUG] Risultati Totali: {totalItems}");

        // 🎯 Passiamo solo i dati necessari alla View
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.SortBy = sortBy;
        ViewBag.TypeFilter = typeFilter;
        ViewBag.PassedFilter = passedFilter;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
        ViewBag.DeviceId = device.Id;
        ViewBag.DeviceName = device.Name;

        return View("~/Views/InterventionHistory/List.cshtml", interventions);
    }
}