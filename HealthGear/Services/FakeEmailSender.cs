using Microsoft.AspNetCore.Identity.UI.Services;

namespace HealthGear.Services;

/// <summary>
/// Implementazione "finta" di IEmailSender, che non invia email reali
/// ma logga semplicemente l'operazione per evitare errori.
/// </summary>
public class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
    /// <summary>
    /// Stub che non invia email reali, ma logga l'operazione.
    /// </summary>
    /// <param name="email">Indirizzo email di destinazione</param>
    /// <param name="subject">Oggetto dell'email</param>
    /// <param name="htmlMessage">Corpo dell'email (HTML)</param>
    /// <returns></returns>
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation($"[FAKE] Email a: {email}, Oggetto: {subject}, Contenuto: {htmlMessage}");
        return Task.CompletedTask;
    }
}