using System.ComponentModel.DataAnnotations;
using HealthGear.Services;

namespace HealthGear.Models.Config;

/// <summary>
///     Impostazioni SMTP per l'invio delle email.
/// </summary>
public class SmtpConfig
{
    private readonly SecureStorage? _secureStorage;

    private string _password = "examplepassword";

    private string _username = "example";

    /// <summary>
    ///     Costruttore che accetta `SecureStorage` e lo utilizza per crittografare/decrittografare i dati.
    /// </summary>
    public SmtpConfig(SecureStorage secureStorage)
    {
        _secureStorage = secureStorage;
    }

    /// <summary>
    ///     Costruttore vuoto richiesto da Entity Framework.
    /// </summary>
    public SmtpConfig()
    {
    }

    [Key] public int Id { get; set; }

    [Required] public string Host { get; set; } = "smtp.healthgear.local";

    [Required] public int Port { get; set; } = 587;

    [Required]
    public string Username
    {
        get => _secureStorage?.DecryptUsername(_username) ?? _username;
        set => _username = _secureStorage?.EncryptUsername(value) ?? value;
    }

    [Required]
    public string Password
    {
        get => _secureStorage?.DecryptPassword(_password) ?? _password;
        set => _password = _secureStorage?.EncryptPassword(value) ?? value;
    }

    public bool UseSsl { get; set; } = true;

    public bool RequiresAuthentication { get; set; } = true;

    [Required] public string SenderName { get; set; } = "Default Sender";

    [Required] [EmailAddress] public string SenderEmail { get; set; } = "no-reply@example.com";
}