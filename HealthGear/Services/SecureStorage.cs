using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace HealthGear.Services;

/// <summary>
///     Servizio per la crittografia sicura delle credenziali SMTP.
/// </summary>
public class SecureStorage
{
    private readonly IDataProtector _passwordProtector;
    private readonly IDataProtector _usernameProtector;
    private readonly ILogger<SecureStorage> _logger;
    private const string EncryptionPrefix = "HG_ENC_"; // Prefisso per identificare le stringhe crittografate

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
    public static bool IsEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(EncryptionPrefix)) return false;
        return TryParseBase64(value.Substring(EncryptionPrefix.Length));
    }

    /// <summary>
    ///     Verifica se una stringa è un valore Base64 valido.
    /// </summary>
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
    public string EncryptPassword(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        var encrypted = _passwordProtector.Protect(plainText);
        return EncryptionPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
    }

    public string EncryptUsername(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        var encrypted = _usernameProtector.Protect(plainText);
        return EncryptionPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
    }

    /// <summary>
    ///     Decrittografa la stringa della password se è crittografata, altrimenti restituisce il valore originale.
    /// </summary>
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
                _logger.LogInformation("🔄 Tentativo di decrittazione multipla...");
                var base64Part = currentText.Substring(EncryptionPrefix.Length);
                var decodedBytes = Convert.FromBase64String(base64Part);
                var decrypted = Encoding.UTF8.GetString(decodedBytes);
                currentText = _passwordProtector.Unprotect(decrypted);
            }

            _logger.LogInformation("✅ Password completamente decrittata: {Password}", currentText);
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
            _logger.LogInformation("✅ Username decrittato con successo.");
            return finalUsername;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Errore durante la decrittografia dello username: {Message}", ex.Message);
            return string.Empty;
        }
    }
}