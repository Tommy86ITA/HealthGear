namespace HealthGear.Models.Settings;

/// <summary>
///     Definisce le regole di validità delle password utilizzate sia da Identity che dal generatore di password custom.
/// </summary>
public class PasswordRules
{
    /// <summary>
    ///     Lunghezza minima della password.
    /// </summary>
    public int MinLength { get; set; } = 8;

    /// <summary>
    ///     Richiede almeno un carattere maiuscolo.
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    ///     Richiede almeno un carattere minuscolo.
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    ///     Richiede almeno una cifra numerica.
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    ///     Richiede almeno un carattere non alfanumerico (es. simboli come @, #, !).
    /// </summary>
    public bool RequireNonAlphanumeric { get; set; } = true;
}