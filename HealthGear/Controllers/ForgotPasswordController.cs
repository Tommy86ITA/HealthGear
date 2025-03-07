using HealthGear.Models;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

/// <summary>
///     Controller per la gestione della procedura di recupero password tramite invio di un'email con link di reset.
/// </summary>
[Route("[controller]")]
public class ForgotPasswordController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    ///     Inizializza una nuova istanza di <see cref="ForgotPasswordController" />.
    /// </summary>
    /// <param name="userManager">Gestore degli utenti di Identity.</param>
    /// <param name="logger">Logger per il tracciamento delle operazioni.</param>
    /// <param name="emailSender">Servizio per l'invio delle email.</param>
    public ForgotPasswordController(
        UserManager<ApplicationUser> userManager,
        ILogger<ForgotPasswordController> logger,
        IEmailSender emailSender)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    /// <summary>
    ///     Mostra la pagina per inserire l'email e avviare la procedura di recupero password.
    /// </summary>
    /// <returns>La vista con il form per la richiesta di recupero password.</returns>
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View(new ForgotPasswordViewModel());
    }

    /// <summary>
    ///     Elabora la richiesta di recupero password.
    ///     Se l'email esiste, genera un token di reset e invia un'email con il link di reset.
    /// </summary>
    /// <param name="model">Il modello con l'email inserita dall'utente.</param>
    /// <returns>Redirect alla stessa pagina con un messaggio informativo.</returns>
    [HttpPost("Index")]
    public async Task<IActionResult> Index(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Non riveliamo se l'email esiste o meno per evitare enumeration attacks
            TempData["InfoMessage"] = "Se l'email è registrata riceverai un'email con le istruzioni per il reset.";
            return RedirectToAction("Index");
        }

        // Genera token di reset
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Crea link di reset
        var resetLink = Url.Action("Reset", "ResetPassword", new { email = user.Email, token }, Request.Scheme);

        _logger.LogInformation("Reset link generato per l'utente {UserId}: {Link}", user.Id, resetLink);

        if (resetLink != null && user.UserName != null)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "{{ResetLink}}", resetLink },
                { "{{UserName}}", user.UserName }
            };

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email!,
                    "Recupero Password - HealthGear",
                    "ResetPasswordEmailTemplate", // Nome del file template senza estensione
                    placeholders
                );

                _logger.LogInformation("Email di recupero password inviata all'utente {UserId} ({Email})", user.Id,
                    user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore durante l'invio dell'email di recupero password all'utente {UserId} ({Email})", user.Id,
                    user.Email);
                TempData["ErrorMessage"] = "Si è verificato un errore durante l'invio dell'email. Riprova più tardi.";
                return RedirectToAction("Index");
            }
        }

        // Messaggio generico per non dare indicazioni sugli utenti esistenti
        TempData["InfoMessage"] = "Se l'indirizzo è registrato riceverai un'email con le istruzioni per il reset.";
        return RedirectToAction("Index");
    }
}