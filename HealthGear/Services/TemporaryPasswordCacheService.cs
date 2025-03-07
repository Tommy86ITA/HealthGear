namespace HealthGear.Services;

public class TemporaryPasswordCacheService
{
    private readonly Dictionary<string, (string Password, DateTime Expiry)> _cache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(2); // Scade dopo 2 minuti

    public void StorePassword(string userName, string password)
    {
        _cache[userName] = (password, DateTime.UtcNow.Add(_cacheDuration));
    }

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