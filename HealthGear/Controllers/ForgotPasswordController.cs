using HealthGear.Models;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

/// <summary>
///     Controller per la gestione della procedura di recupero password tramite invio di un'email con codice di verifica.
/// </summary>
[Route("[controller]/Index")]
public class ForgotPasswordController(
    UserManager<ApplicationUser> userManager,
    ILogger<ForgotPasswordController> logger,
    IEmailSender emailSender)
    : Controller
{
    private static readonly Dictionary<string, DateTime> ResetRequestTracker = new();
    private readonly IEmailSender _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));

    private readonly ILogger<ForgotPasswordController> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly UserManager<ApplicationUser> _userManager =
        userManager ?? throw new ArgumentNullException(nameof(userManager));

    /// <summary>
    ///     Carica la view per l'inserimento dell'email per il recupero della password.
    /// </summary>
    /// <returns>La vista per il recupero della password.</returns>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View(new ForgotPasswordViewModel());
    }

    /// <summary>
    ///     Elabora la richiesta di recupero password con protezione avanzata, limitando il numero di richieste.
    /// </summary>
    /// <param name="model">Il modello contenente l'email dell'utente.</param>
    /// <returns>Reindirizza alla pagina di richiesta con un messaggio di conferma o errore.</returns>
    [HttpPost("")]
    [ValidateAntiForgeryToken] // 🔒 Protezione CSRF
    public async Task<IActionResult> Index(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (IsRateLimited(model.Email))
        {
            TempData["ErrorMessage"] = "Hai già effettuato una richiesta di reset di recente. Attendi qualche minuto.";
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            await Task.Delay(1000); // 🔒 Protezione contro Enumeration Attack
            TempData["InfoMessage"] = "Se l'email è registrata riceverai un'email con le istruzioni per il reset.";
            return RedirectToAction("Index");
        }

        // Genera il token di reset
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        user.StoredResetToken = resetToken;
        user.LastPasswordResetRequest = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("Errore durante il salvataggio dei dati di reset per l'utente {Email}", user.Email);
            TempData["ErrorMessage"] = "Errore interno durante la richiesta di reset. Riprova più tardi.";
            return RedirectToAction("Index");
        }

        // Genera il link di reset
        var resetUrl = Url.Action("Reset", "ResetPassword",
            new { token = resetToken, email = user.Email }, Request.Scheme);
        _logger.LogInformation("📧 Link di reset generato: {ResetLink}", resetUrl);

        if (resetUrl == null) return RedirectToAction("Index");
        var placeholders = new Dictionary<string, string>
        {
            { "{{ResetLink}}", resetUrl },
            { "{{UserName}}", user.UserName ?? "Utente" }
        };

        try
        {
            await _emailSender.SendEmailAsync(
                user.Email!,
                "Recupero Password - HealthGear",
                "ResetPasswordEmailTemplate",
                placeholders
            );

            TempData["SuccessMessage"] = "Se l'email è registrata, riceverai un'email con il link per il reset.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'invio dell'email di recupero password per {Email}", user.Email);
            TempData["ErrorMessage"] = "Si è verificato un errore durante l'invio dell'email. Riprova più tardi.";
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    ///     Limita le richieste di reset password per evitare spam e brute-force.
    /// </summary>
    /// <param name="email">L'email dell'utente per cui si sta verificando il rate-limiting.</param>
    /// <returns>True se la richiesta è limitata, altrimenti false.</returns>
    private static bool IsRateLimited(string email)
    {
        if (ResetRequestTracker.TryGetValue(email, out var lastRequest))
            if ((DateTime.UtcNow - lastRequest).TotalMinutes < 5)
                return true;

        ResetRequestTracker[email] = DateTime.UtcNow;
        return false;
    }
}