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
    public string LogLevel { get; set; } = "Verbose";

    /// <summary>
    ///     Percorso della cartella di log.
    /// </summary>
    [Required]
    [MinLength(512)]
    public string LogPath { get; set; } = "./Logs";
}