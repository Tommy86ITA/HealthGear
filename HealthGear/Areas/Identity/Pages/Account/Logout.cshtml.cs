// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using HealthGear.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthGear.Areas.Identity.Pages.Account;

public class LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
    : PageModel
{
    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        // Esegue il logout dell'utente
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");

        // Reindirizza sempre alla pagina di Login, ignorando eventuali returnUrl
        var loginUrl = Url.Page("/Account/Login", new { area = "Identity" });
        if (loginUrl is null) throw new InvalidOperationException("L'URL di login non può essere null.");
        return LocalRedirect(loginUrl);

        // Se preferisci rispettare returnUrl se presente, potresti fare:
        // if (!string.IsNullOrEmpty(returnUrl))
        // {
        //     return LocalRedirect(returnUrl);
        // }
        // else
        // {
        //     return LocalRedirect(Url.Page("/Account/Login", new { area = "Identity" }));
        // }
    }
}