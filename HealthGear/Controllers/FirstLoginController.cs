using HealthGear.Models;
using HealthGear.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

/// <summary>
///     Gestisce il cambio password obbligatorio al primo accesso.
/// </summary>
[Authorize]
[Route("FirstLogin")]
public class FirstLoginController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<FirstLoginController> logger)
    : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    /// <summary>
    ///     Gestisce la visualizzazione della pagina di primo accesso.
    /// </summary>
    /// <param name="userId">L'ID dell'utente che deve cambiare la password.</param>
    /// <returns>La view per il cambio password o il reindirizzamento alla home se non necessario.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Tentativo di accesso a FirstLogin con un ID utente non valido: {UserId}", userId);
            return NotFound();
        }

        if (!user.MustChangePassword)
        {
            logger.LogInformation("L'utente {UserName} ha già cambiato la password, reindirizzamento alla Home.",
                user.UserName);
            return RedirectToAction("Home", "Home");
        }

        logger.LogInformation("L'utente {UserName} deve cambiare la password al primo accesso.", user.UserName);
        var model = new FirstLoginViewModel { UserId = user.Id };
        return View("~/Views/UserManagement/FirstLogin.cshtml", model);
    }

    /// <summary>
    ///     Gestisce il cambio della password per l'utente al primo accesso.
    /// </summary>
    /// <param name="model">Il modello contenente la nuova password e l'ID dell'utente.</param>
    /// <returns>La view per il cambio password o il reindirizzamento alla home se il cambio è avvenuto con successo.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(FirstLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Tentativo di cambio password fallito per l'utente {UserId}: modello non valido.",
                model.UserId);
            return View("~/Views/UserManagement/FirstLogin.cshtml", model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            logger.LogError(
                "Errore critico: l'utente con ID {UserId} non è stato trovato durante il reset della password.",
                model.UserId);
            return NotFound();
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        if (string.IsNullOrEmpty(resetToken))
        {
            logger.LogError("Errore nel generare il token di reset per l'utente {UserId}.", model.UserId);
            ModelState.AddModelError("", "Errore nella generazione del token di reset. Riprova più tardi.");
            return View("~/Views/UserManagement/FirstLogin.cshtml", model);
        }

        var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
        if (result.Succeeded)
        {
            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
            await _signInManager.SignInAsync(user, false);

            logger.LogInformation("L'utente {UserName} ha cambiato con successo la password al primo accesso.",
                user.UserName);
            return RedirectToAction("Home", "Home");
        }

        logger.LogWarning("Errore durante il reset della password per l'utente {UserId}.", user.UserName);
        foreach (var error in result.Errors)
        {
            logger.LogError("Errore: {ErrorDescription}", error.Description);
            ModelState.AddModelError("", error.Description);
        }

        return View("~/Views/UserManagement/FirstLogin.cshtml", model);
    }
}