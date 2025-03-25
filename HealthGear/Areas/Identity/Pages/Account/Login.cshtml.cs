// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using HealthGear.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthGear.Areas.Identity.Pages.Account;

public class LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
    : PageModel
{
    /// <summary>
    ///     Modello dei dati di input per il login.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     Schemi di autenticazione esterni (Google, Microsoft, ecc.).
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>
    ///     URL di ritorno dopo il login.
    /// </summary>
    public string ReturnUrl { get; set; }

    [TempData] public string ErrorMessage { get; set; }

    /// <summary>
    ///     Carica la pagina di login.
    /// </summary>
    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage)) ModelState.AddModelError(string.Empty, ErrorMessage);

        returnUrl ??= Url.Content("~/");

        // Pulisce eventuali cookie di autenticazione esterni (se esistenti)
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Carica i provider di autenticazione esterni (se configurati)
        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    /// <summary>
    ///     Gestisce la POST del form di login.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = [.. (await signInManager.GetExternalAuthenticationSchemesAsync())];

        if (!ModelState.IsValid) return Page();

        var user = await signInManager.UserManager.FindByNameAsync(Input.UserName);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Nome utente o password errati.");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, false);

        if (result.Succeeded)
        {
            logger.LogInformation("User {UserName} logged in.", user.UserName);
            logger.LogInformation($"[DEBUG] MustChangePassword per {user.UserName}: {user.MustChangePassword}");

            if (user.MustChangePassword)
            {
                //_logger.LogInformation("User {UserName} is required to change password on first login.");
                logger.LogInformation("Reindirizzamento a FirstLogin per userId: {UserId}", user.Id);
                return RedirectToAction("Index", "FirstLogin", new { userId = user.Id });
            }

            user.LastLoginDate = DateTime.UtcNow;
            await signInManager.UserManager.UpdateAsync(user);

            logger.LogInformation("Reindirizzamento a {ReturnUrl}", returnUrl);
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });

        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Tentativo di accesso non valido.");
        return Page();
    }

    /// <summary>
    ///     Modello per i dati di input.
    /// </summary>
    public class InputModel
    {
        [Required(ErrorMessage = "Il nome utente è obbligatorio.")]
        [Display(Name = "Nome utente")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "La password è obbligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Ricordami")] public bool RememberMe { get; set; }
    }
}