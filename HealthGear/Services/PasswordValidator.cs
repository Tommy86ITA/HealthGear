namespace HealthGear.Services;

/// <summary>
///     Servizio dedicato alla validazione delle password in base alle regole definite per il progetto.
/// </summary>
public class PasswordValidator
{
    /// <summary>
    ///     Verifica se la password fornita rispetta i criteri di sicurezza definiti.
    /// </summary>
    /// <param name="password">La password da validare.</param>
    /// <returns>Un oggetto ValidationResult che indica se la password è valida e contiene eventuali errori.</returns>
    public static ValidationResult Validate(string password)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(password))
        {
            result.IsValid = false;
            result.Errors.Add("La password non può essere vuota.");
            return result;
        }

        if (password.Length < 8)
        {
            result.IsValid = false;
            result.Errors.Add("La password deve contenere almeno 8 caratteri.");
        }

        if (!password.Any(char.IsUpper))
        {
            result.IsValid = false;
            result.Errors.Add("La password deve contenere almeno una lettera maiuscola.");
        }

        if (!password.Any(char.IsDigit))
        {
            result.IsValid = false;
            result.Errors.Add("La password deve contenere almeno un numero.");
        }

        if (password.Any(ch => !char.IsLetterOrDigit(ch))) return result;
        result.IsValid = false;
        result.Errors.Add("La password deve contenere almeno un carattere speciale.");

        return result;
    }

    /// <summary>
    ///     Risultato della validazione di una password.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        ///     Indica se la password è conforme alle regole.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        ///     Elenco degli errori rilevati durante la validazione.
        /// </summary>
        public List<string> Errors { get; set; } = [];
    }
}