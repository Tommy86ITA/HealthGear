using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models.Config;

/// <summary>
///     Configurazioni per il logging dell'applicazione.
/// </summary>
public class LoggingConfig
{
    [Key] public int Id { get; set; }

    /// <summary>
    ///     Livello di dettaglio dei log (Verbose, Warning, Error).
    /// </summary>
    [Required]
    public LogLevelEnum LogLevel { get; set; } = LogLevelEnum.Information;
}

/// <summary>
/// Enum per definire i livelli di log ammessi.
/// </summary>
public enum LogLevelEnum
{
    Information,
    Warning,
    Error
}