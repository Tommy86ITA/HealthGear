using Microsoft.AspNetCore.Identity;

namespace HealthGear.Services;

/// <summary>
///     Descrittore personalizzato per i messaggi di errore di Identity in lingua italiana.
///     Compatibile con ASP.NET Core Identity 8 e 9.
/// </summary>
public class ItalianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return new IdentityError
            { Code = nameof(DefaultError), Description = "Si è verificato un errore sconosciuto." };
    }

    public override IdentityError ConcurrencyFailure()
    {
        return new IdentityError
        {
            Code = nameof(ConcurrencyFailure),
            Description = "Errore di concorrenza. L'oggetto è stato modificato da un altro processo."
        };
    }

    public override IdentityError PasswordMismatch()
    {
        return new IdentityError { Code = nameof(PasswordMismatch), Description = "Password non corretta." };
    }

    public override IdentityError InvalidToken()
    {
        return new IdentityError { Code = nameof(InvalidToken), Description = "Token non valido." };
    }

    public override IdentityError LoginAlreadyAssociated()
    {
        return new IdentityError
        {
            Code = nameof(LoginAlreadyAssociated), Description = "Un utente con questo accesso esterno è già associato."
        };
    }

    public override IdentityError InvalidUserName(string? userName)
    {
        return new IdentityError
            { Code = nameof(InvalidUserName), Description = $"Il nome utente '{userName}' non è valido." };
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return new IdentityError
            { Code = nameof(InvalidEmail), Description = $"L'indirizzo email '{email}' non è valido." };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
            { Code = nameof(DuplicateUserName), Description = $"Il nome utente '{userName}' è già in uso." };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
            { Code = nameof(DuplicateEmail), Description = $"L'indirizzo email '{email}' è già in uso." };
    }

    public override IdentityError InvalidRoleName(string? role)
    {
        return new IdentityError
            { Code = nameof(InvalidRoleName), Description = $"Il nome ruolo '{role}' non è valido." };
    }

    public override IdentityError DuplicateRoleName(string role)
    {
        return new IdentityError
            { Code = nameof(DuplicateRoleName), Description = $"Il nome ruolo '{role}' è già in uso." };
    }

    public override IdentityError UserAlreadyHasPassword()
    {
        return new IdentityError
            { Code = nameof(UserAlreadyHasPassword), Description = "L'utente ha già una password impostata." };
    }

    public override IdentityError UserLockoutNotEnabled()
    {
        return new IdentityError
            { Code = nameof(UserLockoutNotEnabled), Description = "Il blocco dell'utente non è abilitato." };
    }

    public override IdentityError UserAlreadyInRole(string role)
    {
        return new IdentityError
            { Code = nameof(UserAlreadyInRole), Description = $"L'utente appartiene già al ruolo '{role}'." };
    }

    public override IdentityError UserNotInRole(string role)
    {
        return new IdentityError
            { Code = nameof(UserNotInRole), Description = $"L'utente non appartiene al ruolo '{role}'." };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
            { Code = nameof(PasswordTooShort), Description = $"La password deve contenere almeno {length} caratteri." };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "La password deve contenere almeno un carattere non alfanumerico."
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
            { Code = nameof(PasswordRequiresDigit), Description = "La password deve contenere almeno un numero." };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = "La password deve contenere almeno una lettera minuscola."
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "La password deve contenere almeno una lettera maiuscola."
        };
    }

    public override IdentityError RecoveryCodeRedemptionFailed()
    {
        return new IdentityError
            { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Il codice di recupero non è valido." };
    }
}