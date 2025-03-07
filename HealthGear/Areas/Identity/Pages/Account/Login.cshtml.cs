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

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

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
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    /// <summary>
    ///     Gestisce la POST del form di login.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid) return Page();
        // Prova il login usando UserName anziché Email
        var result =
            await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            // Recupera l'utente e aggiorna LastLoginDate
            var user = await _signInManager.UserManager.FindByNameAsync(Input.UserName);
            if (user == null) return LocalRedirect(returnUrl);
            user.LastLoginDate = DateTime.UtcNow;
            await _signInManager.UserManager.UpdateAsync(user);
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Tentativo di accesso non valido.");

        // Se qualcosa è andato storto, rispedisce l'utente alla pagina con i messaggi di errore.
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