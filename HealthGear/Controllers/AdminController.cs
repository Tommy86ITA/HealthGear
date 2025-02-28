using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

public class AdminController : Controller
{
    // GET: /Admin/AdminTasks
    [HttpGet]
    public IActionResult AdminTasks()
    {
        // Potresti passare un modello alla view se desideri
        return View();
    }
}