using HealthGear.Models.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HealthGear.Services;

/// <summary>
///     Servizio per l'invio di email utilizzando MailKit.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceProvider _serviceProvider; // Aggiunto per SecureStorage
    private readonly SettingsService _settingsService;
    private SmtpConfig _smtpConfig;

    /// <summary>
    ///     Costruttore che riceve il servizio impostazioni e il logger.
    /// </summary>
    /// <param name="settingsService">Servizio per il recupero delle impostazioni dal database.</param>
    /// <param name="logger">Logger per registrare eventuali errori o informazioni.</param>
    /// <param name="serviceProvider">Provider dei servizi per accedere a SecureStorage.</param>
    /// // Aggiunto parametro
    public EmailSender(SettingsService settingsService, ILogger<EmailSender> logger,
        IServiceProvider serviceProvider) // Modificato costruttore
    {
        _settingsService = settingsService;
        _logger = logger;
        _serviceProvider = serviceProvider; // Inizializzato

        // Valori di fallback per evitare problemi in caso di database corrotto o vuoto
        var secureStorage = serviceProvider.GetRequiredService<SecureStorage>();
        _smtpConfig = new SmtpConfig(secureStorage)
        {
            Host = "smtp.example.com",
            Port = 587,
            SenderEmail = "noreply@example.com",
            SenderName = "HealthGear",
            Username = "username",
            Password = "",
            RequiresAuthentication = true,
            UseSsl = true
        };

        LoadSettings().Wait();
    }

    /// <summary>
    ///     Invia un'email utilizzando un template HTML con segnaposto.
    /// </summary>
    public async Task SendEmailAsync(string toEmail, string subject, string templateName,
        Dictionary<string, string> placeholders)
    {
        _logger.LogInformation(
            "Preparazione invio email a {Recipient} con oggetto '{Subject}' usando template '{Template}'",
            toEmail, subject, templateName);

        try
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", $"{templateName}.html");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Il template email '{templatePath}' non è stato trovato.");

            var body = await File.ReadAllTextAsync(templatePath);
            body = placeholders.Aggregate(body,
                (current, placeholder) => current.Replace(placeholder.Key, placeholder.Value));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpConfig.SenderName, _smtpConfig.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            _logger.LogInformation("Connessione al server SMTP {Host}:{Port}...", _smtpConfig.Host, _smtpConfig.Port);

            await smtp.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port, SecureSocketOptions.StartTls);

            if (!string.IsNullOrWhiteSpace(_smtpConfig.Username) && !string.IsNullOrWhiteSpace(_smtpConfig.Password))
            {
                _logger.LogInformation("Autenticazione in corso per l'utente {Username}...", _smtpConfig.Username);
                await smtp.AuthenticateAsync(_smtpConfig.Username, _smtpConfig.Password);
            }
            else
            {
                _logger.LogWarning("Attenzione: invio senza autenticazione. Il server potrebbe rifiutare l'email.");
            }

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email inviata con successo a {Recipient}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'invio dell'email a {Recipient}.", toEmail);
            throw;
        }
    }

    /// <summary>
    ///     Carica le impostazioni SMTP dal database.
    /// </summary>
    private async Task LoadSettings()
    {
        var config = await _settingsService.GetConfigAsync();
        if (config?.Smtp == null)
        {
            _logger.LogCritical("Le impostazioni SMTP non sono valide.");
            throw new InvalidOperationException("Configurazione SMTP non valida.");
        }

        // Le credenziali SMTP vengono decriptate qui una volta sola e memorizzate in _smtpConfig.
        // I getter delle credenziali NON eseguono la decrittazione automaticamente.
        // Dopo questa chiamata, Username e Password saranno già in chiaro per l'uso nei metodi di EmailSender.
        var secureStorage = _serviceProvider.GetRequiredService<SecureStorage>();
        config.Smtp.Username = secureStorage.DecryptUsername(config.Smtp.Username); // 🔓 Decrittografa Username
        config.Smtp.Password = secureStorage.DecryptPassword(config.Smtp.Password); // 🔓 Decrittografa Password
        _smtpConfig = config.Smtp;

        _logger.LogInformation("Inizializzazione EmailSender con server {Host} sulla porta {Port}.",
            _smtpConfig.Host, _smtpConfig.Port);

        if (!string.IsNullOrWhiteSpace(_smtpConfig.Host) && _smtpConfig.Port != 0) return;
        _logger.LogCritical("Le impostazioni SMTP non sono valide. Verifica il database.");
        throw new InvalidOperationException("Configurazione SMTP non valida.");
    }
    /// <summary>
    ///     Testa la connessione al server SMTP utilizzando i parametri passati dalla View.
    /// </summary>
    public async Task<bool> TestSmtpConnectionAsync(string host, int port, string username, string password, bool useSsl, bool requiresAuth)
    {
        _logger.LogInformation("🔍 Avvio test connessione SMTP con parametri personalizzati...");

        try
        {
            using var smtp = new SmtpClient();
            _logger.LogInformation("🌐 Connessione a {Host}:{Port}...", host, port);

            await smtp.ConnectAsync(host, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            if (requiresAuth)
            {
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogInformation("🔑 Tentativo di autenticazione per l'utente {Username}...", username);
                    await smtp.AuthenticateAsync(username, password);
                }
                else
                {
                    _logger.LogWarning("⚠️ L'autenticazione è richiesta, ma non sono state fornite credenziali valide.");
                    throw new InvalidOperationException("Autenticazione richiesta, ma credenziali non valide.");
                }
            }

            await smtp.DisconnectAsync(true);
            _logger.LogInformation("✅ Connessione SMTP riuscita con i parametri forniti.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Errore durante il test di connessione SMTP.");
            return false;
        }
    }
}