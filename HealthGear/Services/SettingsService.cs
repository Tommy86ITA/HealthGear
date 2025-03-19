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
    ///     Recupera la configurazione corrente dal database e decrittografa Username e Password SMTP.
    ///     Se non è possibile decrittografare, vengono restituiti valori vuoti per Username e Password.
    /// </summary>
    /// <returns>Restituisce l'oggetto AppConfig contenente la configurazione attuale, oppure null se non trovata.</returns>
    public async Task<AppConfig?> GetConfigAsync()
    {
        var config = await context.Configurations
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();
        if (config?.Smtp == null) return config;

        // Decrittografia dello username se è crittografato
        try
        {
            config.Smtp.Username = IsEncrypted(config.Smtp.Username)
                ? secureStorage.DecryptUsername(config.Smtp.Username)
                : config.Smtp.Username;
        }
        catch (Exception ex)
        {
            logger.LogError("Errore durante la decrittografia dello username: {Message}", ex.Message);
            config.Smtp.Username = string.Empty; // Imposta a vuoto in caso di errore
        }

        // Decrittografia della password se è crittografata
        try
        {
            config.Smtp.Password = IsEncrypted(config.Smtp.Password)
                ? secureStorage.DecryptPassword(config.Smtp.Password)
                : config.Smtp.Password;
        }
        catch (Exception ex)
        {
            logger.LogError("Errore durante la decrittografia della password: {Message}", ex.Message);
            config.Smtp.Password = string.Empty; // Imposta a vuoto in caso di errore
        }

        return config;
    }

    /// <summary>
    ///     Aggiorna la configurazione nel database, crittografando Username e Password SMTP.
    ///     Se non esiste una configurazione precedente, viene creata una nuova configurazione.
    /// </summary>
    /// <param name="config">L'oggetto AppConfig contenente la nuova configurazione da salvare.</param>
    public async Task UpdateConfigAsync(AppConfig config)
    {
        // Crittografia dello username se non è già crittografato
        if (!string.IsNullOrEmpty(config.Smtp.Username) && !IsEncrypted(config.Smtp.Username))
            config.Smtp.Username = secureStorage.EncryptUsername(config.Smtp.Username);

        // Crittografia della password se non è già crittografata
        if (!string.IsNullOrEmpty(config.Smtp.Password) && !IsEncrypted(config.Smtp.Password))
            config.Smtp.Password = secureStorage.EncryptPassword(config.Smtp.Password);

        var existingConfig = await context.Configurations.FirstOrDefaultAsync();
        if (existingConfig == null)
            context.Configurations.Add(config); // Aggiungi la nuova configurazione se non esiste
        else
            context.Entry(existingConfig).CurrentValues.SetValues(config); // Aggiorna la configurazione esistente

        await context.SaveChangesAsync(); // Salva le modifiche nel database
    }

    /// <summary>
    ///     Verifica se il valore è crittografato controllando se è una stringa Base64 valida.
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
}