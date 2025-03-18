using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

[AllowAnonymous]
[Route("StatusPages")]
public class StatusController : Controller
{
    /// <summary>
    ///     Restituisce la pagina di errore per l'accesso negato (403).
    /// </summary>
    /// <returns>La vista della pagina di errore.</returns>
    [Route("AccessDenied")]
    public IActionResult AccessDenied()
    {
        ViewBag.ErrorCode = "403";
        ViewBag.ErrorMessage = "Accesso negato. Non hai i permessi per accedere a questa pagina.";
        return View("ErrorPage");
    }

    /// <summary>
    ///     Restituisce la pagina di errore per la risorsa non trovata (404).
    /// </summary>
    /// <returns>La vista della pagina di errore.</returns>
    [Route("NotFound")]
    public IActionResult NotFoundPage()
    {
        ViewBag.ErrorCode = "404";
        ViewBag.ErrorMessage = "Pagina non trovata. Il contenuto richiesto potrebbe essere stato spostato o eliminato.";
        return View("ErrorPage");
    }

    /// <summary>
    ///     Restituisce la pagina di errore generico (500).
    /// </summary>
    /// <returns>La vista della pagina di errore.</returns>
    [Route("Error")]
    public IActionResult GenericError()
    {
        ViewBag.ErrorCode = "500";
        ViewBag.ErrorMessage = "Si è verificato un errore imprevisto. Riprova più tardi o contatta l'assistenza.";
        return View("ErrorPage");
    }
}