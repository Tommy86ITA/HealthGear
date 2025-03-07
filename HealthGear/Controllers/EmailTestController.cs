using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthGear.Controllers;

[Route("EmailTest")]
public class EmailTestController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailTestController> _logger;

    public EmailTestController(IEmailSender emailSender, ILogger<EmailTestController> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    [HttpGet("SendTestEmail")]
    public async Task<IActionResult> SendTestEmail()
    {
        try
        {
            _logger.LogInformation("Avvio test invio email...");

            var placeholders = new Dictionary<string, string>
            {
                { "ResetLink", "https://localhost:5001/fake-reset-link" }
            };

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates",
                "ResetPasswordEmailTemplate.html");

            await _emailSender.SendEmailAsync("test@healthgear.local", "Test Email HealthGear", templatePath,
                placeholders);

            _logger.LogInformation("Email di test inviata con successo.");
            return Ok("Email di test inviata! Controlla MailHog.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante invio email di test.");
            return StatusCode(500, $"Errore: {ex.Message}");
        }
    }
}