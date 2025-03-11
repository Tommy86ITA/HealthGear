using HealthGear.Models.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HealthGear.Services;

/// <summary>
///     Servizio per l'invio di email utilizzando MailKit.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailSender> _logger;

    /// <summary>
    ///     Costruttore che riceve le impostazioni email e il logger.
    /// </summary>
    /// <param name="emailSettings">Le impostazioni SMTP lette da appsettings.json.</param>
    /// <param name="logger">Logger per registrare eventuali errori o informazioni.</param>
    public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;

        _logger.LogInformation("Inizializzazione EmailSender con server {Host} sulla porta {Port}.",
            _emailSettings.SmtpServer, _emailSettings.Port);

        if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpServer) && _emailSettings.Port != 0) return;
        _logger.LogCritical("Le impostazioni SMTP non sono valide. Verifica appsettings.json.");
        throw new InvalidOperationException("Configurazione SMTP non valida.");
    }

    /// <summary>
    ///     Invia un'email utilizzando un template HTML con segnaposto.
    /// </summary>
    /// <param name="toEmail">Indirizzo email del destinatario.</param>
    /// <param name="subject">Oggetto dell'email.</param>
    /// <param name="templateName">Nome del template HTML (senza estensione .html).</param>
    /// <param name="placeholders">Dizionario di segnaposto e valori da sostituire nel template.</param>
    public async Task SendEmailAsync(string toEmail, string subject, string templateName,
        Dictionary<string, string> placeholders)
    {
        _logger.LogInformation(
            "Preparazione invio email a {Recipient} con oggetto '{Subject}' usando template '{Template}'", toEmail,
            subject, templateName);

        try
        {
            // Percorso completo del template
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", $"{templateName}.html");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Il template email '{templatePath}' non è stato trovato.");

            // Lettura del file HTML
            var body = await File.ReadAllTextAsync(templatePath);

            // Sostituzione dei placeholder nel template
            foreach (var placeholder in placeholders)
            {
                body = body.Replace(placeholder.Key, placeholder.Value);
            }

            // Composizione messaggio
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            message.Body = builder.ToMessageBody();

            // Invio tramite MailKit
            using var smtp = new SmtpClient();
            _logger.LogInformation("Connessione al server SMTP {Host}:{Port}...", _emailSettings.SmtpServer,
                _emailSettings.Port);

            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);

            if (!string.IsNullOrWhiteSpace(_emailSettings.Username) &&
                !string.IsNullOrWhiteSpace(_emailSettings.Password))
            {
                _logger.LogInformation("Autenticazione in corso per l'utente {Username}...", _emailSettings.Username);
                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
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
            throw; // Propaga l'errore per gestirlo a livello superiore
        }
    }
}