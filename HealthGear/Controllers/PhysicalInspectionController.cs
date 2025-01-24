using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthGear.Data;
using HealthGear.Models;

namespace HealthGear.Controllers;

public class PhysicalInspectionController : Controller
{
    private readonly ApplicationDbContext _context;

    public PhysicalInspectionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PhysicalInspection/Index
    public async Task<IActionResult> Index(int deviceId)
    {
        var inspections = await _context.PhysicalInspections
            .Where(p => p.DeviceId == deviceId)
            .Include(p => p.Device)
            .ToListAsync();

        if (!inspections.Any())
        {
            TempData["ErrorMessage"] = "Nessun controllo fisico disponibile per questo dispositivo.";
            return RedirectToAction("Index", "Device");
        }

        ViewBag.DeviceId = deviceId;
        return View(inspections);
    }

    // GET: PhysicalInspection/Create
    public IActionResult Create(int deviceId)
    {
        ViewBag.DeviceId = deviceId;
        return View();
    }

    // POST: PhysicalInspection/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PhysicalInspection physicalInspection)
    {
        if (ModelState.IsValid)
        {
            _context.Add(physicalInspection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { deviceId = physicalInspection.DeviceId });
        }

        ViewBag.DeviceId = physicalInspection.DeviceId;
        return View(physicalInspection);
    }

    // GET: PhysicalInspection/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var inspection = await _context.PhysicalInspections
            .Include(p => p.Device)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inspection == null) return NotFound();

        return View(inspection);
    }

    // GET: PhysicalInspection/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var inspection = await _context.PhysicalInspections.FindAsync(id);
        if (inspection == null) return NotFound();
        return View(inspection);
    }

    // POST: PhysicalInspection/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PhysicalInspection physicalInspection)
    {
        if (id != physicalInspection.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(physicalInspection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { deviceId = physicalInspection.DeviceId });
        }

        return View(physicalInspection);
    }

    // GET: PhysicalInspection/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var inspection = await _context.PhysicalInspections.FindAsync(id);
        if (inspection == null) return NotFound();
        return View(inspection);
    }

    // POST: PhysicalInspection/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inspection = await _context.PhysicalInspections.FindAsync(id);
        if (inspection != null)
        {
            _context.PhysicalInspections.Remove(inspection);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { deviceId = inspection.DeviceId });
    }
}