namespace HealthGear.Services;

/// <summary>
///     Servizio per la gestione temporanea delle password generate, con una durata limitata.
/// </summary>
public class TemporaryPasswordCacheService
{
    private readonly Dictionary<string, (string Password, DateTime Expiry)> _cache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(2); // Scade dopo 2 minuti

    /// <summary>
    ///     Memorizza temporaneamente una password generata per un utente, con una scadenza predefinita.
    /// </summary>
    /// <param name="userName">Il nome utente associato alla password.</param>
    /// <param name="password">La password temporanea da memorizzare.</param>
    public void StorePassword(string userName, string password)
    {
        _cache[userName] = (password, DateTime.UtcNow.Add(_cacheDuration));
    }

    /// <summary>
    ///     Recupera la password temporanea per un utente, se non è ancora scaduta.
    /// </summary>
    /// <param name="userName">Il nome utente per cui recuperare la password.</param>
    /// <returns>La password temporanea se ancora valida, altrimenti null.</returns>
    public string? RetrievePassword(string userName)
    {
        if (!_cache.TryGetValue(userName, out var entry)) return null;
        if (DateTime.UtcNow <= entry.Expiry)
        {
            _cache.Remove(userName); // Rimuoviamo subito dopo l'uso
            return entry.Password;
        }

        // Se è scaduta, la rimuoviamo
        _cache.Remove(userName);

        return null;
    }
}