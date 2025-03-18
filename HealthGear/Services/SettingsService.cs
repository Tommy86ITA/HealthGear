using HealthGear.Data;
using HealthGear.Models.Config;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Services;

/// <summary>
///     Gestisce il caricamento e l'aggiornamento delle impostazioni dal database.
/// </summary>
public class SettingsService(SettingsDbContext context, SecureStorage secureStorage, ILogger<SettingsService> logger)
{
    /// <summary>
    /// Verifica se il valore è crittografato controllando se è una stringa Base64 valida.
    /// </summary>
    private static bool IsEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;

        try
        {
            _ = Convert.FromBase64String(value); // Assegniamo il risultato a `_` per evitare il warning
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Recupera la configurazione corrente dal database e decrittografa Username e Password SMTP.
    /// </summary>
    public async Task<AppConfig?> GetConfigAsync()
    {
        var config = await context.Configurations
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();
        if (config?.Smtp == null) return config;

        try
        {
            config.Smtp.Username = IsEncrypted(config.Smtp.Username)
                ? secureStorage.DecryptUsername(config.Smtp.Username)
                : config.Smtp.Username;
        }
        catch (Exception ex)
        {
            logger.LogError("Errore durante la decrittografia dello username: {Message}", ex.Message);
            config.Smtp.Username = string.Empty;
        }

        try
        {
            config.Smtp.Password = IsEncrypted(config.Smtp.Password)
                ? secureStorage.DecryptPassword(config.Smtp.Password)
                : config.Smtp.Password;
        }
        catch (Exception ex)
        {
            logger.LogError("Errore durante la decrittografia della password: {Message}", ex.Message);
            config.Smtp.Password = string.Empty;
        }

        return config;
    }

    /// <summary>
    ///     Aggiorna la configurazione nel database, crittografando Username e Password SMTP.
    /// </summary>
    public async Task UpdateConfigAsync(AppConfig config)
    {
        if (!string.IsNullOrEmpty(config.Smtp.Username) && !IsEncrypted(config.Smtp.Username))
            config.Smtp.Username = secureStorage.EncryptUsername(config.Smtp.Username);

        if (!string.IsNullOrEmpty(config.Smtp.Password) && !IsEncrypted(config.Smtp.Password))
            config.Smtp.Password = secureStorage.EncryptPassword(config.Smtp.Password);

        var existingConfig = await context.Configurations.FirstOrDefaultAsync();
        if (existingConfig == null)
            context.Configurations.Add(config);
        else
            context.Entry(existingConfig).CurrentValues.SetValues(config);

        await context.SaveChangesAsync();
    }
}