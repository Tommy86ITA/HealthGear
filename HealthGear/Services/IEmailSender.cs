namespace HealthGear.Services;

/// <summary>
///     Interfaccia per il servizio di invio email.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    ///     Invia un'email utilizzando un template HTML.
    /// </summary>
    /// <param name="toEmail">Indirizzo email del destinatario.</param>
    /// <param name="subject">Oggetto dell'email.</param>
    /// <param name="templatePath">Percorso al template HTML.</param>
    /// <param name="placeholders">Segnaposto da sostituire nel template.</param>
    Task SendEmailAsync(string toEmail, string subject, string templatePath, Dictionary<string, string> placeholders);
}