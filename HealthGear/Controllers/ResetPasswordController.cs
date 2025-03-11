using HealthGear.Models;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

/// <summary>
/// Controller per la gestione del reset della password tramite token inviato via email.
/// </summary>
[Route("[controller]/Reset")]
public class ResetPasswordController : Controller
{
    private readonly ILogger<ResetPasswordController> _logger;
    private readonly PasswordValidator _passwordValidator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Inizializza una nuova istanza del <see cref="ResetPasswordController"/>.
    /// </summary>
    /// <param name="userManager">Gestore degli utenti.</param>
    /// <param name="logger">Logger per il tracciamento delle operazioni.</param>
    /// <param name="passwordValidator">Servizio per la validazione delle password.</param>
    /// <param name="signInManager">Gestore delle autenticazioni.</param>
    public ResetPasswordController(
        UserManager<ApplicationUser> userManager,
        ILogger<ResetPasswordController> logger,
        PasswordValidator passwordValidator,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    }

    /// <summary>
    /// Mostra la schermata di reset password con il modulo di inserimento.
    /// </summary>
    /// <param name="email">L'email dell'utente.</param>
    /// <param name="token">Il token di reset ricevuto via email.</param>
    /// <returns>La view di reset password.</returns>
    [HttpGet("")]
    public IActionResult Reset(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Tentativo di accesso a Reset senza email o token validi.");
            return BadRequest("Richiesta non valida.");
        }

        _logger.LogInformation("Tentativo di reset password con email: {Email} e token: {Token}", email, token);

        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };

        return View(model);
    }

    /// <summary>
    /// Processa la richiesta di reset della password inviata dall'utente.
    /// </summary>
    /// <param name="model">Il modello contenente i dati inseriti dall'utente.</param>
    /// <returns>
    /// Redirect alla pagina di login in caso di successo; altrimenti ritorna alla view con messaggi di errore.
    /// </returns>
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validazione del formato del token
        if (!IsTokenValidFormat(model.Token))
        {
            _logger.LogWarning("Formato del token non valido: {Token}", model.Token);
            ModelState.AddModelError(string.Empty, "Il token di reset non è valido.");
            return View(model);
        }

        // Validazione password con il servizio centralizzato
        var validationResult = _passwordValidator.Validate(model.NewPassword);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning("Tentato reset password per email non esistente: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Utente non trovato.");
            return View(model);
        }

        _logger.LogDebug("Ricerca utente per reset password: {Email}", model.Email);

        // Reset della password tramite token
        var resetResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (resetResult.Succeeded)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!resetResult.Succeeded)
        {
            _logger.LogError("Errore nel reset della password per l'utente {Email}: {Errors}", user.Email, string.Join(", ", resetResult.Errors.Select(e => e.Description)));
        }

        foreach (var error in resetResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    /// <summary>
    /// Verifica se il token di reset ha un formato valido (base64-url).
    /// </summary>
    /// <param name="token">Il token da validare.</param>
    /// <returns><c>true</c> se il token è valido, altrimenti <c>false</c>.</returns>
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