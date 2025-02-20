#region

using HealthGear.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HealthGear.Controllers;

public class InterventionHistoryController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> List(int deviceId, string typeFilter, string passedFilter, DateTime? dateFrom,
        DateTime? dateTo, string sortBy, int page = 1)
    {
        // ✅ Controlliamo se il dispositivo esiste
        var device = await context.Devices
            .Where(d => d.Id == deviceId)
            .Select(d => new
                { d.Id, d.Name, d.Brand, d.Model, d.SerialNumber, d.InventoryNumber }) // ✅ Aggiunti i campi mancanti
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

        // Dati per comporre il titolo della pagina
        ViewBag.DeviceName = device.Name;
        ViewBag.DeviceBrand = device.Brand;
        ViewBag.DeviceModel = device.Model;
        ViewBag.DeviceSerialNumber = device.SerialNumber;
        ViewBag.DeviceInventoryNumber = device.InventoryNumber;

        return View("~/Views/InterventionHistory/List.cshtml", interventions);
    }
}