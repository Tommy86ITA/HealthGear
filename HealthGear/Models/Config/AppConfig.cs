using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthGear.Services;

namespace HealthGear.Models.Config;

/// <summary>
///     Classe contenitore per tutte le configurazioni.
/// </summary>
[Table("AppConfig")]
public class AppConfig
{
    /// <summary>
    ///     Costruttore vuoto richiesto da EF Core.
    /// </summary>
    public AppConfig()
    {
    }

    /// <summary>
    ///     Costruttore che accetta `SecureStorage` e lo passa a `SmtpConfig`.
    /// </summary>
    public AppConfig(SecureStorage secureStorage)
    {
        Smtp = new SmtpConfig(secureStorage);
    }

    [Key] public int Id { get; set; }

    /// <summary>
    ///     Configurazioni SMTP.
    /// </summary>
    public required SmtpConfig Smtp { get; set; }

    /// <summary>
    ///     Configurazioni Logging.
    /// </summary>
    public required LoggingConfig Logging { get; set; } = new();
}