using HealthGear.Data;
using HealthGear.Models.Config;
using HealthGear.Models.ViewModels;
using HealthGear.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers.Admin;

/// <summary>
///     Controller per la gestione delle impostazioni generali dell'applicazione.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminSettingsController(
    SettingsService settingsService,
    SettingsDbContext dbContext,
    SecureStorage secureStorage,
    ILogger<AdminSettingsController> logger)
    : Controller
{
    /// <summary>
    ///     Mostra la pagina delle impostazioni generali dell'applicazione.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var config = await settingsService.GetConfigAsync() ?? new AppConfig
        {
            Smtp = new SmtpConfig(secureStorage),
            Logging = new LoggingConfig()
        };

        // Decrittografiamo Username e Password prima di passarli al ViewModel
        if (!string.IsNullOrEmpty(config.Smtp.Username) && SecureStorage.IsEncrypted(config.Smtp.Username))
            config.Smtp.Username = secureStorage.DecryptUsername(config.Smtp.Username);

        if (!string.IsNullOrEmpty(config.Smtp.Password) && SecureStorage.IsEncrypted(config.Smtp.Password))
        {
            config.Smtp.Password = secureStorage.DecryptPassword(config.Smtp.Password);
        }

        var model = new AdminSettingsViewModel(config)
        {
            Smtp = new SmtpConfig(secureStorage)
            {
                Host = config.Smtp.Host,
                Port = config.Smtp.Port,
                SenderEmail = config.Smtp.SenderEmail,
                SenderName = config.Smtp.SenderName,
                Username = SecureStorage.IsEncrypted(config.Smtp.Username)
                    ? secureStorage.DecryptUsername(config.Smtp.Username)
                    : config.Smtp.Username,
                Password = SecureStorage.IsEncrypted(config.Smtp.Password)
                    ? secureStorage.DecryptPassword(config.Smtp.Password)
                    : config.Smtp.Password,
                UseSsl = config.Smtp.UseSsl,
                RequiresAuthentication = config.Smtp.RequiresAuthentication
            },
            Logging = config.Logging
        };
        return View(model);
    }

    /// <summary>
    ///     Aggiorna le impostazioni generali dell'applicazione.
    /// </summary>
    /// <param name="model">Il modello contenente le nuove impostazioni da salvare.</param>
    /// <returns>Un'azione che rappresenta il risultato dell'aggiornamento delle impostazioni.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(AdminSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        // Se la password è già crittografata, non la crittografiamo di nuovo
        if (SecureStorage.IsEncrypted(model.Smtp.Password))
        {
            logger.LogWarning("⚠️ ATTENZIONE: La password sembra già crittografata prima del salvataggio!");
        }

        var passwordToSave = SecureStorage.IsEncrypted(model.Smtp.Password)
            ? model.Smtp.Password
            : secureStorage.EncryptPassword(model.Smtp.Password);

        var updatedSmtpConfig = new SmtpConfig(secureStorage)
        {
            Host = model.Smtp.Host,
            Port = model.Smtp.Port,
            SenderEmail = model.Smtp.SenderEmail,
            SenderName = model.Smtp.SenderName,
            Username = SecureStorage.IsEncrypted(model.Smtp.Username)
                ? model.Smtp.Username
                : secureStorage.EncryptUsername(model.Smtp.Username),
            Password = passwordToSave,
            UseSsl = model.Smtp.UseSsl,
            RequiresAuthentication = model.Smtp.RequiresAuthentication
        };

        var existingConfig = await dbContext.Configurations
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();
        if (existingConfig != null)
        {
            existingConfig.Smtp = updatedSmtpConfig;
            existingConfig.Logging = model.Logging;
        }
        else
        {
            dbContext.Configurations.Add(new AppConfig
            {
                Smtp = updatedSmtpConfig,
                Logging = model.Logging
            });
        }
        
        await dbContext.SaveChangesAsync();
        return Json(new { success = true, title = "Successo", message = "Impostazioni salvate con successo!" });
    }

    /// <summary>
    ///     Testa la connessione SMTP con i parametri forniti.
    /// </summary>
    /// <param name="smtpConfig">I parametri SMTP da testare.</param>
    /// <param name="emailSender">Il servizio per l'invio delle email.</param>
    /// <returns>Un'azione che rappresenta il risultato del test della connessione SMTP.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestSmtp([FromBody] SmtpConfig smtpConfig, [FromServices] EmailSender emailSender)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "I parametri inseriti non sono validi." });
        }

        // Testiamo la connessione SMTP con i parametri forniti
        var success = await emailSender.TestSmtpConnectionAsync(
            smtpConfig.Host,
            smtpConfig.Port,
            smtpConfig.Username,
            smtpConfig.Password,
            smtpConfig.UseSsl,
            smtpConfig.RequiresAuthentication
        );

        if (success)
        {
            return Json(new { success = true, message = "Connessione SMTP riuscita!" });
        }

        return Json(
            new { success = false, message = "Errore nella connessione SMTP. Controlla i parametri e riprova." });
    }
}