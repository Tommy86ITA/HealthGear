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

        logger.LogInformation("🔍 Valore password prima della decrittazione: {Password}", config.Smtp.Password);

        // Decrittografiamo Username e Password prima di passarli al ViewModel
        if (!string.IsNullOrEmpty(config.Smtp.Username) && SecureStorage.IsEncrypted(config.Smtp.Username))
            config.Smtp.Username = secureStorage.DecryptUsername(config.Smtp.Username);

        if (!string.IsNullOrEmpty(config.Smtp.Password) && SecureStorage.IsEncrypted(config.Smtp.Password))
        {
            logger.LogInformation("🔍 La password è crittografata, tentativo di decrittazione...");
            config.Smtp.Password = secureStorage.DecryptPassword(config.Smtp.Password);
            logger.LogInformation("🔓 Password dopo decrittazione: {Password}", config.Smtp.Password);

            // Controlliamo che la decrittazione abbia funzionato
            if (SecureStorage.IsEncrypted(config.Smtp.Password))
                logger.LogWarning("⚠️ Attenzione: La password decrittata sembra ancora crittografata!");
        }
        else
        {
            logger.LogInformation("⚠️ La password non è crittografata, nessuna decrittazione eseguita.");
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(AdminSettingsViewModel model)
    {
        logger.LogInformation("🔍 Ricevuti dati da form: Host={Host}, Porta={Port}, Username={Username}, Password={Password}, SenderName={SenderName}, SenderEmail={SenderEmail}, UseSsl={UseSsl}, RequiresAuthentication={RequiresAuth}, LogLevel={LogLevel}",
            model?.Smtp?.Host,
            model?.Smtp?.Port,
            model?.Smtp?.Username,
            model?.Smtp?.Password,
            model?.Smtp?.SenderName,
            model?.Smtp?.SenderEmail,
            model?.Smtp?.UseSsl,
            model?.Smtp?.RequiresAuthentication,
            model?.Logging?.LogLevel
        );

        if (!ModelState.IsValid)
        {
            logger.LogWarning("⚠️ Il modello non è valido!");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    logger.LogWarning("❌ Errore nel campo {Key}: {ErrorMessage}", key, error.ErrorMessage);
                }
            }
            return View("Index", model);
        }

        logger.LogInformation("🔍 Stato iniziale password ricevuta dal form: {Password}", model.Smtp.Password);

        // Se la password è già crittografata, non la crittografiamo di nuovo
        if (SecureStorage.IsEncrypted(model.Smtp.Password))
            logger.LogWarning("⚠️ ATTENZIONE: La password sembra già crittografata prima del salvataggio!");

        var passwordToSave = SecureStorage.IsEncrypted(model.Smtp.Password)
            ? model.Smtp.Password
            : secureStorage.EncryptPassword(model.Smtp.Password);

        logger.LogInformation("🔐 Password finale che verrà salvata: {Password}", passwordToSave);

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

        var existingConfig = await dbContext.Configurations.FirstOrDefaultAsync();
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

        logger.LogInformation("Valore Username salvato nel DB: {Username}", model.Smtp.Username);

        await dbContext.SaveChangesAsync();
        return Json(new { success = true, title = "Successo", message = "Impostazioni salvate con successo!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestSmtp([FromBody] SmtpConfig smtpConfig, [FromServices] EmailSender emailSender)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("⚠️ Parametri SMTP non validi per il test.");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    logger.LogWarning("❌ Errore nel campo {Key}: {ErrorMessage}", key, error.ErrorMessage);
                }
            }
            return Json(new { success = false, message = "I parametri inseriti non sono validi." });
        }

        logger.LogInformation("🔍 Avvio test connessione SMTP con i parametri ricevuti...");

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
            logger.LogInformation("✅ Test connessione SMTP riuscito.");
            return Json(new { success = true, message = "Connessione SMTP riuscita!" });
        }

        logger.LogWarning("❌ Test connessione SMTP fallito.");
        return Json(new { success = false, message = "Errore nella connessione SMTP. Controlla i parametri e riprova." });
    }
}