using HealthGear.Constants;
using HealthGear.Models;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

/// <summary>
///     Controller per la gestione degli utenti. Accessibile solo agli utenti con ruolo Admin.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public class UserManagementController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    PasswordGenerator passwordGenerator,
    TemporaryPasswordCacheService passwordCacheService)
    : Controller
{
    private readonly TemporaryPasswordCacheService _passwordCacheService = passwordCacheService;

    /// <summary>
    ///     Mostra la lista di tutti gli utenti.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users.ToListAsync();
        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Nessun ruolo";
            userViewModels.Add(UserViewModel.FromApplicationUser(user, primaryRole));
        }

        return View(userViewModels);
    }

    /// <summary>
    ///     Mostra il form di creazione di un nuovo utente.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new UserViewModel
        {
            AvailableRoles = await GetAvailableRolesAsync()
        };

        return View(model);
    }

    /// <summary>
    ///     Elabora la creazione di un nuovo utente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await GetAvailableRolesAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            FullName = model.FullName,
            Email = model.Email,
            IsActive = true,
            RegistrationDate = DateTime.UtcNow,
            MustChangePassword = true // Forziamo il cambio password al primo login
        };

        var generatedPassword = passwordGenerator.Generate();
        var result = await userManager.CreateAsync(user, generatedPassword);

        if (!result.Succeeded)
        {
            model.AvailableRoles = await GetAvailableRolesAsync();
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        await userManager.AddToRoleAsync(user, model.Role);

        // Salviamo la password nella cache temporanea anziché in TempData
        _passwordCacheService.StorePassword(user.UserName, generatedPassword);

        return RedirectToAction(nameof(UserCreatedConfirmation), new { userName = user.UserName });
    }

    /// <summary>
    ///     Mostra la conferma di creazione utente, con la password generata.
    /// </summary>
    [HttpGet]
    public IActionResult UserCreatedConfirmation(string userName)
    {
        var generatedPassword = _passwordCacheService.RetrievePassword(userName);
        Console.WriteLine(
            $"[DEBUG] Password recuperata per {userName}: {(string.IsNullOrEmpty(generatedPassword) ? "NESSUNA PASSWORD TROVATA" : generatedPassword)}");

        if (string.IsNullOrEmpty(generatedPassword))
        {
            TempData["ErrorMessage"] = "La password temporanea non è più disponibile o è scaduta.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["UserName"] = userName;
        ViewData["GeneratedPassword"] = generatedPassword;

        return View();
    }

    /// <summary>
    ///     Mostra il form di modifica di un utente esistente.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var (user, primaryRole) = await GetUserWithPrimaryRoleAsync(id);
        if (user == null) return NotFound();

        var model = UserViewModel.FromApplicationUser(user, primaryRole);
        model.GeneratedPassword = TempData["NewPassword"]?.ToString();
        model.AvailableRoles = await GetAvailableRolesAsync();

        return View(model);
    }

    /// <summary>
    ///     Mostra i dettagli di un utente esistente, inclusi dati di registrazione, stato e ultime attività.
    /// </summary>
    /// <param name="id">L'ID dell'utente da visualizzare.</param>
    /// <returns>Un <see cref="IActionResult" /> che visualizza la pagina dei dettagli utente.</returns>
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        // Recupera l'utente e il suo ruolo principale
        var (user, primaryRole) = await GetUserWithPrimaryRoleAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utente non trovato.";
            return RedirectToAction(nameof(Index));
        }

        // Crea il ViewModel con tutte le informazioni da mostrare
        var model = UserViewModel.FromApplicationUser(user, primaryRole);
        model.AvailableRoles = await GetAvailableRolesAsync();

        return View(model);
    }

    /// <summary>
    ///     Salva le modifiche a un utente esistente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserViewModel model)
    {
        if (id != model.Id) return BadRequest();

        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (IsCurrentUser(user.Id) && await userManager.IsInRoleAsync(user, Roles.Admin) && model.Role != Roles.Admin)
        {
            TempData["ErrorMessage"] = "Non puoi rimuovere a te stesso il ruolo di amministratore.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await GetAvailableRolesAsync();
            return View(model);
        }

        model.UpdateApplicationUser(user);
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Contains(model.Role)) return RedirectToAction(nameof(Index));
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction(nameof(Index));
        }

        model.AvailableRoles = await GetAvailableRolesAsync();
        return View(model);
    }

    /// <summary>
    ///     Elimina definitivamente un utente dal sistema.
    /// </summary>
    /// <param name="id">ID dell'utente da eliminare.</param>
    /// <returns>Un <see cref="IActionResult" /> che reindirizza alla lista utenti.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utente non trovato.";
            return RedirectToAction(nameof(Index));
        }

        if (IsCurrentUser(user.Id))
        {
            TempData["ErrorMessage"] = "Non puoi eliminare il tuo stesso account.";
            return RedirectToAction(nameof(Index));
        }

        var adminCount = (await userManager.GetUsersInRoleAsync(Roles.Admin)).Count;
        var isAdmin = await userManager.IsInRoleAsync(user, Roles.Admin);

        if (isAdmin && adminCount <= 1)
        {
            TempData["ErrorMessage"] = "Non puoi eliminare l'ultimo amministratore.";
            return RedirectToAction(nameof(Index));
        }

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = $"Errore durante l'eliminazione dell'utente {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"L'utente {user.UserName} è stato eliminato.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     Disattiva un utente, registrando il motivo e la data di disattivazione.
    /// </summary>
    /// <param name="userId">Identificativo dell'utente da disattivare.</param>
    /// <param name="deactivationReason">Motivo della disattivazione.</param>
    /// <returns>Redirect alla lista utenti.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string userId, string deactivationReason)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utente non trovato.";
            return RedirectToAction("Index");
        }

        user.IsActive = false;
        user.DeactivationDate = DateTime.Now;
        user.DeactivationReason = deactivationReason;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Errore durante la disattivazione dell'utente.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = $"Utente {user.FullName} disattivato con successo.";
        return RedirectToAction("Index");
    }

    /// <summary>
    ///     Riattiva un utente precedentemente disattivato, ripristinando lo stato attivo e rimuovendo eventuali motivi e date
    ///     di disattivazione.
    /// </summary>
    /// <param name="Id">
    ///     Identificatore univoco dell'utente da riattivare. Il nome di questo parametro deve rimanere "id"
    ///     per garantire la corretta associazione con il parametro della rotta definito in Program.cs.
    ///     Modificare questo nome causerebbe il fallimento del model binding e il metodo non riceverebbe il valore corretto.
    /// </param>
    /// <returns>
    ///     Un <see cref="IActionResult" /> che reindirizza alla lista utenti con un messaggio di conferma o errore.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    // ReSharper disable once InconsistentNaming
    public async Task<IActionResult> Reactivate(string Id)
    {
        if (string.IsNullOrEmpty(Id))
        {
            TempData["ErrorMessage"] = "ID utente non valido.";
            return RedirectToAction(nameof(Index));
        }

        // Recupera l'utente dal database tramite l'UserManager
        var user = await userManager.FindByIdAsync(Id);

        // Se l'utente non esiste, mostra un messaggio di errore
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utente non trovato.";
            return RedirectToAction(nameof(Index));
        }

        // Controllo di sicurezza: non permettere la riattivazione di te stesso (se richiesto dalle policy)
        if (IsCurrentUser(user.Id))
        {
            TempData["ErrorMessage"] = "Non puoi modificare direttamente lo stato del tuo account.";
            return RedirectToAction(nameof(Index));
        }

        // Aggiorna lo stato dell'utente
        user.IsActive = true;
        user.DeactivationDate = null;
        user.DeactivationReason = null;

        // Esegui aggiornamento sul database
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = $"Errore durante la riattivazione dell'utente {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        // Messaggio di conferma
        TempData["SuccessMessage"] = $"L'utente {user.FullName} è stato riattivato con successo.";

        // Reindirizza alla lista degli utenti
        return RedirectToAction(nameof(Index));
    }


    /// <summary>
    ///     Reimposta la password di un utente esistente, genera una nuova password temporanea
    ///     e reindirizza a una pagina di conferma dove l'admin può visualizzare la nuova password generata.
    /// </summary>
    /// <param name="id">ID dell'utente di cui eseguire il reset della password.</param>
    /// <returns>Un <see cref="IActionResult" /> che reindirizza alla pagina di conferma.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Utente non trovato.";
            return RedirectToAction(nameof(Index));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var newPassword = passwordGenerator.Generate();

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Errore durante il reset della password.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        _passwordCacheService.StorePassword(user.UserName!, newPassword);

        return RedirectToAction(nameof(PasswordResetConfirmation), new { userName = user.UserName });
    }

    /// <summary>
    ///     Mostra la pagina di conferma del reset password, con la nuova password generata mostrata all'admin.
    /// </summary>
    /// <param name="userName">Lo username dell'utente di cui è stata resettata la password.</param>
    /// <returns>Un <see cref="IActionResult" /> che visualizza la pagina di conferma.</returns>
    [HttpGet]
    [HttpGet]
    public IActionResult PasswordResetConfirmation(string userName)
    {
        var newPassword = _passwordCacheService.RetrievePassword(userName);

        if (string.IsNullOrEmpty(newPassword))
        {
            TempData["ErrorMessage"] = "La password temporanea non è più disponibile. È possibile che sia scaduta.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["UserName"] = userName;
        ViewData["NewPassword"] = newPassword;

        return View();
    }

    /// <summary>
    ///     Verifica se l'ID utente specificato corrisponde all'utente attualmente autenticato.
    /// </summary>
    /// <param name="userId">L'ID dell'utente da confrontare con l'utente attualmente autenticato.</param>
    /// <returns>True se l'ID corrisponde all'utente autenticato, altrimenti False.</returns>
    private bool IsCurrentUser(string userId)
    {
        return userId == userManager.GetUserId(User);
    }

    /// <summary>
    ///     Recupera l'elenco completo dei ruoli disponibili nel sistema.
    /// </summary>
    /// <returns>Lista di stringhe contenente i nomi di tutti i ruoli esistenti.</returns>
    private async Task<List<string>> GetAvailableRolesAsync()
    {
        return await roleManager.Roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .ToListAsync();
    }


    /// <summary>
    ///     Recupera un utente e il suo ruolo principale.
    /// </summary>
    /// <param name="userId">L'ID dell'utente da cercare.</param>
    /// <returns>Tuple con ApplicationUser e il ruolo primario (o stringa vuota se nessun ruolo trovato).</returns>
    private async Task<(ApplicationUser? user, string primaryRole)> GetUserWithPrimaryRoleAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return (null, string.Empty);

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Nessun ruolo";

        return (user, primaryRole);
    }
}