namespace HealthGear.Models.Config;

/// <summary>
/// Modello di configurazione per healthgear.config.json
/// </summary>
public class HealthGearConfig
{
    public int ServerPort { get; set; }
    public string DatabasePath { get; set; } = string.Empty;
    public string UploadFolderPath { get; set; } = string.Empty;
    public string AllowedHosts { get; set; } = string.Empty;
}