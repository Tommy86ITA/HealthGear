// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using HealthGear.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace HealthGear.Areas.Identity.Pages.Account;

public class RegisterModel(
    UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore,
    SignInManager<ApplicationUser> signInManager,
    ILogger<RegisterModel> logger,
    IEmailSender emailSender)
    : PageModel
{
    [BindProperty] public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid) return Page();
        var user = CreateUser();

        // Imposta i campi personalizzati
        user.FullName = Input.FullName;
        user.IsActive = true;
        user.RegistrationDate = DateTime.UtcNow;

        // Qui salviamo lo UserName direttamente, eliminando il legame con l'Email
        await userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
        // Se vuoi conservare l'email per eventuali notifiche interne, puoi tenerla
        if (userManager.SupportsUserEmail)
            await ((IUserEmailStore<ApplicationUser>)userStore).SetEmailAsync(user,
                Input.Email ?? $"{Input.UserName}@healthgear.local", CancellationToken.None);

        var result = await userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("User created a new account with password.");

            TempData["SuccessMessage"] = "Account creato con successo! Benvenuto su HealthGear.";

            if (userManager.Options.SignIn.RequireConfirmedAccount)
            {
                var userId = await userManager.GetUserIdAsync(user);
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    null,
                    new { area = "Identity", userId, code, returnUrl },
                    Request.Scheme);

                if (callbackUrl != null)
                    await emailSender.SendEmailAsync(Input.Email, "Conferma il tuo account",
                        $"Conferma il tuo account cliccando <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>qui</a>.");

                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
            }

            await signInManager.SignInAsync(user, false);
            return LocalRedirect(returnUrl);
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

        // Se ci sono errori, torna alla pagina con i messaggi di errore
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Il nome utente è obbligatorio.")]
        [Display(Name = "Nome utente")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Il nome completo è obbligatorio.")]
        [Display(Name = "Nome completo")]
        public string FullName { get; set; }

        // Email diventa opzionale, ma puoi anche rimuoverla del tutto
        [EmailAddress]
        [Display(Name = "Email (opzionale)")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La {0} deve essere almeno {2} e al massimo {1} caratteri.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Conferma password")]
        [Compare("Password", ErrorMessage = "La password e la conferma password non coincidono.")]
        public string ConfirmPassword { get; set; }
    }
}