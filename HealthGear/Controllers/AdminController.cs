using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

[Authorize]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // GET: /Admin/AdminTasks
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminTasks()
    {
        // Potresti passare un modello alla view se desideri
        return View();
    }
}