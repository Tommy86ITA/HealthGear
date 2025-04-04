using HealthGear.Models.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HealthGear.Models.ViewModels;

/// <summary>
///     ViewModel per la gestione delle impostazioni generali dell'applicazione.
///     Contiene solo le informazioni necessarie alla vista e garantisce la validazione dei dati.
/// </summary>
public class AdminSettingsViewModel
{
    /// <summary>
    ///     Costruttore predefinito richiesto per il binding del modello.
    /// </summary>
    public AdminSettingsViewModel()
    {
        Smtp = new SmtpConfig();
        Logging = new LoggingConfig
        {
            LogLevel = LogLevelEnum.Information // Valore predefinito
        };
    }

    /// <summary>
    ///     Costruttore che permette di inizializzare il ViewModel con i dati esistenti.
    /// </summary>
    public AdminSettingsViewModel(AppConfig config)
    {
        Smtp = new SmtpConfig
        {
            Host = config.Smtp.Host,
            Port = config.Smtp.Port,
            SenderEmail = config.Smtp.SenderEmail,
            SenderName = config.Smtp.SenderName,
            RequiresAuthentication = config.Smtp.RequiresAuthentication,
            UseSsl = config.Smtp.UseSsl,
            Username = config.Smtp.Username,
            Password = config.Smtp.Password
        };

        Logging = new LoggingConfig
        {
            LogLevel = config.Logging.LogLevel // Usa direttamente l'enum
        };
    }

    /// <summary>
    ///     Impostazioni SMTP.
    /// </summary>
    [BindProperty]
    [BindRequired]
    public required SmtpConfig Smtp { get; set; }

    /// <summary>
    ///     Impostazioni di logging.
    /// </summary>
    [BindProperty]
    [BindRequired]
    public required LoggingConfig Logging { get; set; }
}