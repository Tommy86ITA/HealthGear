using HealthGear.Models;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

/// <summary>
///     Controller per la gestione del reset della password tramite token inviato via email.
/// </summary>
public class ResetPasswordController : Controller
{
    private readonly ILogger<ResetPasswordController> _logger;
    private readonly PasswordValidator _passwordValidator;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    ///     Inizializza una nuova istanza del <see cref="ResetPasswordController" />.
    /// </summary>
    /// <param name="userManager">Gestore degli utenti.</param>
    /// <param name="logger">Logger per il tracciamento delle operazioni.</param>
    /// <param name="passwordValidator">Servizio per la validazione delle password.</param>
    public ResetPasswordController(
        UserManager<ApplicationUser> userManager,
        ILogger<ResetPasswordController> logger,
        PasswordValidator passwordValidator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
    }

    /// <summary>
    ///     Mostra la schermata di reset password.
    /// </summary>
    /// <param name="email">L'email dell'utente.</param>
    /// <param name="token">Il token di reset ricevuto via email.</param>
    /// <returns>La view di reset password.</returns>
    [HttpGet]
    public IActionResult Reset(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Tentativo di accesso a Reset senza email o token validi.");
            return BadRequest("Richiesta non valida.");
        }

        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };

        return View(model);
    }

    /// <summary>
    ///     Processa il reset della password inviato dal form.
    /// </summary>
    /// <param name="model">Il modello con i dati inseriti dall'utente.</param>
    /// <returns>Redirect alla login in caso di successo, altrimenti torna alla view.</returns>
    [HttpPost]
    public async Task<IActionResult> Reset(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Validazione formato token (base64-url - opzionale ma consigliato)
        if (!IsTokenValidFormat(model.Token))
        {
            ModelState.AddModelError(string.Empty, "Il token di reset non è valido.");
            return View(model);
        }

        // Validazione password tramite il nuovo servizio centralizzato
        var validationResult = PasswordValidator.Validate(model.NewPassword);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors) ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning("Tentato reset password per email non esistente: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Utente non trovato.");
            return View(model);
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Password reimpostata con successo. Ora puoi accedere con la nuova password.";
            return Redirect("/Identity/Account/Login");
        }

        // Mostriamo eventuali errori di Identity (es. token non valido o scaduto)
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

        _logger.LogWarning("Errore reset password per utente {UserId}: {Errors}", user.Id,
            string.Join(", ", result.Errors.Select(e => e.Description)));

        return View(model);
    }

    /// <summary>
    ///     Valida il formato del token (controlla che sia base64-url valido).
    /// </summary>
    /// <param name="token">Il token da validare.</param>
    /// <returns>true se il token è valido, altrimenti false.</returns>
    private static bool IsTokenValidFormat(string token)
    {
        try
        {
            var bytes = Convert.FromBase64String(token.Replace('-', '+').Replace('_', '/'));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}