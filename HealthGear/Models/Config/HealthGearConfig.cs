using System.Runtime.InteropServices;

namespace HealthGear.Models.Config;

/// <summary>
/// Modello di configurazione per healthgear.config.json
/// </summary>
public class HealthGearConfig
{
    /// <summary>
    /// Porta del server HTTP.
    /// </summary>
    public int ServerPort { get; set; }

    /// <summary>
    /// Percorso al file del database principale.
    /// </summary>
    public required string DatabasePath { get; set; }

    /// <summary>
    /// Percorso della cartella per i file caricati.
    /// </summary>
    public required string UploadFolderPath { get; set; }

    /// <summary>
    /// Host autorizzati per le richieste.
    /// </summary>
    public required string AllowedHosts { get; set; }

    /// <summary>
    /// Percorso al file settings.db, usato per configurazioni avanzate o di debug.
    /// Se non specificato, verrà usato il valore di default.
    /// </summary>
    public required string SettingsDbPath { get; set; }

    /// <summary>
    /// Crea una configurazione di default in base al sistema operativo.
    /// </summary>
    public static HealthGearConfig CreateDefault()
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        string basePath = isWindows
            ? @"C:\ProgramData\HealthGear"
            : AppContext.BaseDirectory.TrimEnd('/');

        return new HealthGearConfig
        {
            ServerPort = 5001,
            DatabasePath = Path.Combine(basePath, "healthgear.db"),
            SettingsDbPath = Path.Combine(basePath, "settings.db"),
            UploadFolderPath = isWindows
                ? @"C:\HealthGear\Uploads"
                : Path.Combine(basePath, "uploads"),
            AllowedHosts = "localhost,0.0.0.0,[::]"
        };
    }
}