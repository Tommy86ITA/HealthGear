namespace HealthGear.Models.Config;

/// <summary>
/// Modello di configurazione per healthgear.config.json
/// </summary>
public class HealthGearConfig
{
    public int ServerPort { get; set; } = 5001;
    public string DatabasePath { get; set; } = "healthgear.db";
    public string UploadFolderPath { get; set; } = "Uploads";
    public string AllowedHosts { get; set; } = "localhost,0.0.0.0,[::]";

    /// <summary>
    /// Percorso al file settings.db, usato per configurazioni avanzate o di debug.
    /// Se non specificato, verrà usato il valore di default.
    /// </summary>
    public string? SettingsDbPath { get; set; } = null;
}