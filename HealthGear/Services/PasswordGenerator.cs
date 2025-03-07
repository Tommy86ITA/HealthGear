using HealthGear.Models.Settings;

namespace HealthGear.Services;

/// <summary>
///     Servizio per la generazione di password casuali conformi alle regole configurate,
///     con un metodo aggiuntivo per ottenere la descrizione leggibile dei requisiti.
/// </summary>
public class PasswordGenerator
{
    private readonly PasswordRules _rules;

    /// <summary>
    ///     Inizializza una nuova istanza di <see cref="PasswordGenerator" /> con le regole specificate.
    /// </summary>
    /// <param name="rules">Le regole da applicare alla generazione della password.</param>
    public PasswordGenerator(PasswordRules rules)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    ///     Genera una password casuale conforme alle regole configurate.
    /// </summary>
    /// <returns>Una password generata in modo casuale, conforme ai criteri definiti in <see cref="PasswordRules" />.</returns>
    public string Generate()
    {
        const string uppercase = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialCharacters = "!@$?_-";

        var random = new Random();

        // Costruisce il pool di caratteri consentiti in base alle regole.
        var allChars = string.Empty;
        if (_rules.RequireUppercase) allChars += uppercase;
        if (_rules.RequireLowercase) allChars += lowercase;
        if (_rules.RequireDigit) allChars += digits;
        if (_rules.RequireNonAlphanumeric) allChars += specialCharacters;

        if (string.IsNullOrEmpty(allChars))
            throw new InvalidOperationException("Nessun set di caratteri valido è stato configurato.");

        // Inizializza la password con la lunghezza minima richiesta.
        var password = new char[_rules.MinLength];

        var i = 0;

        // Garantisce che almeno un carattere per ogni requisito venga incluso.
        if (_rules.RequireUppercase)
            password[i++] = uppercase[random.Next(uppercase.Length)];

        if (_rules.RequireLowercase)
            password[i++] = lowercase[random.Next(lowercase.Length)];

        if (_rules.RequireDigit)
            password[i++] = digits[random.Next(digits.Length)];

        if (_rules.RequireNonAlphanumeric)
            password[i++] = specialCharacters[random.Next(specialCharacters.Length)];

        // Completa la password con caratteri casuali dal pool consentito.
        for (; i < _rules.MinLength; i++)
            password[i] = allChars[random.Next(allChars.Length)];

        // Rimescola la password per evitare pattern prevedibili (es. sempre le regole in ordine).
        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }

    /// <summary>
    ///     Restituisce una stringa descrittiva con i requisiti della password configurati.
    /// </summary>
    /// <returns>Stringa con i requisiti leggibili della password.</returns>
    public string GetPasswordRequirementsDescription()
    {
        var requirements = new List<string>
        {
            $"Lunghezza minima: {_rules.MinLength} caratteri"
        };

        if (_rules.RequireUppercase)
            requirements.Add("Almeno una lettera maiuscola");

        if (_rules.RequireLowercase)
            requirements.Add("Almeno una lettera minuscola");

        if (_rules.RequireDigit)
            requirements.Add("Almeno un numero");

        if (_rules.RequireNonAlphanumeric)
            requirements.Add("Almeno un carattere speciale");

        return string.Join(", ", requirements);
    }
}