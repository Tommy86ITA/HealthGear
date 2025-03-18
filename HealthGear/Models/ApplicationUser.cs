using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace HealthGear.Models;

/// <summary>
///     Rappresenta l'utente dell'applicazione.
///     Estende IdentityUser per integrare il sistema di autenticazione di ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    ///     Nome completo dell'utente.
    /// </summary>
    [Required(ErrorMessage = "Il nome completo è obbligatorio.")]
    [MaxLength(100, ErrorMessage = "Il nome completo non può superare 100 caratteri.")]
    public required string FullName { get; set; }

    /// <summary>
    ///     Indica se l'account è attivo.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Data di registrazione.
    /// </summary>
    public DateTime RegistrationDate { get; set; }

    /// <summary>
    ///     Data dell'ultimo login.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    ///     Data di eventuale disattivazione dell'account.
    /// </summary>
    public DateTime? DeactivationDate { get; set; }

    /// <summary>
    ///     Motivo della disattivazione.
    /// </summary>
    [MaxLength(250)]
    public string? DeactivationReason { get; set; }

    /// <summary>
    ///     Indica se l'utente deve cambiare la password al prossimo accesso.
    /// </summary>
    public bool MustChangePassword { get; set; } = true;

    /// <summary>
    ///     Token di reset della password memorizzato per la verifica.
    /// </summary>
    public string? StoredResetToken { get; set; }

    /// <summary>
    ///     Timestamp dell'ultima richiesta di reset della password.
    /// </summary>
    public DateTime? LastPasswordResetRequest { get; set; }
}