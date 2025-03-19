using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace HealthGear.Services;

/// <summary>
///     Servizio per la crittografia sicura delle credenziali SMTP.
/// </summary>
public class SecureStorage
{
    private const string EncryptionPrefix = "HG_ENC_"; // Prefisso per identificare le stringhe crittografate
    private readonly ILogger<SecureStorage> _logger;
    private readonly IDataProtector _passwordProtector;
    private readonly IDataProtector _usernameProtector;

    /// <summary>
    ///     Costruttore che inizializza il servizio di protezione dati.
    /// </summary>
    /// <param name="provider">Provider di protezione dati di ASP.NET.</param>
    /// <param name="logger">Logger per la gestione degli errori.</param>
    public SecureStorage(IDataProtectionProvider provider, ILogger<SecureStorage> logger)
    {
        _passwordProtector = provider.CreateProtector("SmtpPasswordProtector");
        _usernameProtector = provider.CreateProtector("SmtpUsernameProtector");
        _logger = logger;
    }

    /// <summary>
    ///     Controlla se un valore è crittografato verificando il prefisso e la validità Base64.
    /// </summary>
    /// <param name="value">Il valore da controllare.</param>
    /// <returns>Restituisce true se il valore è crittografato, altrimenti false.</returns>
    public static bool IsEncrypted(string value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith(EncryptionPrefix) && TryParseBase64(value.Substring(EncryptionPrefix.Length));
    }

    /// <summary>
    ///     Verifica se una stringa è un valore Base64 valido.
    /// </summary>
    /// <param name="base64Part">La parte della stringa da verificare.</param>
    /// <returns>Restituisce true se la stringa è un Base64 valido, altrimenti false.</returns>
    private static bool TryParseBase64(string base64Part)
    {
        try
        {
            _ = Convert.FromBase64String(base64Part);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Crittografa una stringa prima di salvarla nel database, aggiungendo il prefisso identificativo.
    /// </summary>
    /// <param name="plainText">La stringa da crittografare.</param>
    /// <returns>Restituisce la stringa crittografata con il prefisso.</returns>
    public string EncryptPassword(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        var encrypted = _passwordProtector.Protect(plainText);
        return EncryptionPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
    }

    /// <summary>
    ///     Crittografa una stringa di username prima di salvarla nel database, aggiungendo il prefisso identificativo.
    /// </summary>
    /// <param name="plainText">La stringa di username da crittografare.</param>
    /// <returns>Restituisce la stringa di username crittografata con il prefisso.</returns>
    public string EncryptUsername(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        var encrypted = _usernameProtector.Protect(plainText);
        return EncryptionPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
    }

    /// <summary>
    ///     Decrittografa la stringa della password se è crittografata, altrimenti restituisce il valore originale.
    /// </summary>
    /// <param name="encryptedText">La stringa crittografata da decrittografare.</param>
    /// <returns>Restituisce la password decrittografata o il valore originale se non è crittografato.</returns>
    public string DecryptPassword(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText) || !IsEncrypted(encryptedText))
        {
            _logger.LogWarning("⚠️ Tentativo di decrittazione su un valore non crittografato: {Value}", encryptedText);
            return encryptedText;
        }

        try
        {
            var currentText = encryptedText;

            // Iteriamo finché la password risulta ancora crittografata
            while (IsEncrypted(currentText))
            {
                var base64Part = currentText.Substring(EncryptionPrefix.Length);
                var decodedBytes = Convert.FromBase64String(base64Part);
                var decrypted = Encoding.UTF8.GetString(decodedBytes);
                currentText = _passwordProtector.Unprotect(decrypted);
            }

            _logger.LogInformation("✅ Password decrittata con successo.");
            return currentText;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Errore durante la decrittografia della password: {Message}", ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    ///     Decrittografa la stringa dello username se è crittografata, altrimenti restituisce il valore originale.
    /// </summary>
    /// <param name="encryptedText">La stringa crittografata da decrittografare.</param>
    /// <returns>Restituisce lo username decrittografato o il valore originale se non è crittografato.</returns>
    public string DecryptUsername(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText) || !IsEncrypted(encryptedText))
        {
            _logger.LogWarning("⚠️ Tentativo di decrittazione su un valore non crittografato: {Value}", encryptedText);
            return encryptedText;
        }

        try
        {
            // Rimuoviamo il prefisso di crittografia
            var base64Part = encryptedText.Substring(EncryptionPrefix.Length);
            var decodedBytes = Convert.FromBase64String(base64Part);
            var decrypted = Encoding.UTF8.GetString(decodedBytes);

            // Ora possiamo decrittografare il valore effettivo
            var finalUsername = _usernameProtector.Unprotect(decrypted);
            return finalUsername;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Errore durante la decrittografia dello username: {Message}", ex.Message);
            return string.Empty;
        }
    }
}