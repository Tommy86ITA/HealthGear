using Ganss.Xss;
using HealthGear.Models;
using HealthGear.Models.Settings;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HealthGear.Controllers;

/// <summary>
///     Controller per la gestione del profilo utente.
///     Consente la modifica delle informazioni personali e della password.
/// </summary>
[Authorize]
public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly PasswordRules _passwordRules;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    ///     Inizializza una nuova istanza del <see cref="ProfileController" />.
    /// </summary>
    /// <param name="userManager">Gestore utenti Identity.</param>
    /// <param name="signInManager">Gestore autenticazione Identity.</param>
    /// <param name="passwordRules">Regole di generazione password lette da configurazione.</param>
    /// <param name="logger">Logger per tracciare eventuali errori.</param>
    public ProfileController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<PasswordRules> passwordRules,
        ILogger<ProfileController> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _passwordRules = passwordRules.Value ?? throw new ArgumentNullException(nameof(passwordRules));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Mostra la pagina di modifica del profilo.
    ///     Recupera e passa anche i requisiti della password configurati.
    /// </summary>
    /// <returns>La vista di modifica profilo.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty
        };

        var passwordGenerator = new PasswordGenerator(_passwordRules);
        ViewData["PasswordRequirements"] = passwordGenerator.GetPasswordRequirementsDescription();

        return View(model);
    }

    /// <summary>
    ///     Salva le modifiche al profilo dell'utente.
    ///     Effettua una sanitizzazione aggressiva per evitare inserimento di HTML.
    /// </summary>
    /// <param name="model">Modello con i dati aggiornati.</param>
    /// <returns>Ritorna alla pagina di modifica profilo.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Verifica di aver compilato correttamente i campi.";
            return RedirectToAction(nameof(Edit));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        try
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedAttributes.Clear();

            user.FullName = sanitizer.Sanitize(model.FullName);
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["ProfileSuccessMessage"] = "Profilo aggiornato con successo.";
                return RedirectToAction(nameof(Edit));
            }

            TempData["ErrorMessage"] = "Errore durante l'aggiornamento del profilo.";
            _logger.LogWarning("Errore aggiornamento profilo utente {UserId}: {Errors}", user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eccezione durante aggiornamento profilo utente {UserId}", user.Id);
            TempData["ErrorMessage"] = "Si è verificato un errore inatteso.";
        }

        return RedirectToAction(nameof(Edit));
    }

    /// <summary>
    ///     Modifica la password dell'utente.
    ///     Esegue un controllo di corrispondenza tra nuova password e conferma.
    /// </summary>
    /// <param name="currentPassword">Password attuale.</param>
    /// <param name="newPassword">Nuova password.</param>
    /// <param name="confirmPassword">Conferma nuova password.</param>
    /// <returns>Ritorna alla pagina di modifica profilo.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "La nuova password e la conferma non coincidono.";
            return RedirectToAction(nameof(Edit));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        try
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["PasswordSuccessMessage"] = "Password aggiornata con successo.";
                return RedirectToAction(nameof(Edit));
            }

            TempData["ErrorMessage"] = "Errore durante l'aggiornamento della password: " +
                                       string.Join(", ", result.Errors.Select(e => TranslateError(e.Description)));

            _logger.LogWarning("Errore aggiornamento password utente {UserId}: {Errors}", user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eccezione durante aggiornamento password utente {UserId}", user.Id);
            TempData["ErrorMessage"] = "Si è verificato un errore inatteso durante l'aggiornamento della password.";
        }

        return RedirectToAction(nameof(Edit));
    }

    /// <summary>
    ///     Converte i messaggi di errore di Identity in italiano o forma leggibile.
    /// </summary>
    /// <param name="error">Testo originale dell'errore.</param>
    /// <returns>Testo tradotto.</returns>
    private static string TranslateError(string error)
    {
        return error switch
        {
            "Passwords must have at least one non alphanumeric character." =>
                "La password deve contenere almeno un carattere speciale.",
            "Passwords must have at least one lowercase ('a'-'z')." =>
                "La password deve contenere almeno una lettera minuscola.",
            "Passwords must have at least one uppercase ('A'-'Z')." =>
                "La password deve contenere almeno una lettera maiuscola.",
            "Passwords must be at least 8 characters." => "La password deve contenere almeno 8 caratteri.",
            "Incorrect password." => "La password attuale non è corretta.",
            _ => error
        };
    }
}