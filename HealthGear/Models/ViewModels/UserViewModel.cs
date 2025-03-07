using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models.ViewModels;

/// <summary>
///     Modello di visualizzazione per la gestione e la visualizzazione dei dettagli di un utente.
/// </summary>
public class UserViewModel
{
    /// <summary>
    ///     Identificatore univoco dell'utente.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Nome completo dell'utente.
    /// </summary>
    [Required]
    [Display(Name = "Nome Completo")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    ///     Username dell'utente.
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Indirizzo email dell'utente.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Ruolo attuale dell'utente.
    /// </summary>
    [Required]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    ///     Indica se l'utente è attivo o disattivato.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Data di registrazione dell'utente.
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    ///     Data e ora dell'ultimo accesso.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    ///     Data della disattivazione dell'utente (se presente).
    /// </summary>
    public DateTime? DeactivationDate { get; set; }

    /// <summary>
    ///     Motivazione della disattivazione (se presente).
    /// </summary>
    public string? DeactivationReason { get; set; }

    /// <summary>
    ///     Lista dei ruoli disponibili da mostrare in una select (usato nelle view di modifica).
    /// </summary>
    public List<string> AvailableRoles { get; set; } = [];

    /// <summary>
    ///     Password generata (usata solo per la visualizzazione dopo il reset o la creazione).
    /// </summary>
    public string? GeneratedPassword { get; set; }

    /// <summary>
    ///     ViewModel per la gestione del modal di disattivazione (usato nella view di modifica utente).
    ///     Viene inizializzato solo quando serve, da FromApplicationUser.
    /// </summary>
    public DeactivateUserModalViewModel? DeactivateModalViewModel { get; set; }

    /// <summary>
    ///     Crea un ViewModel a partire da un ApplicationUser.
    /// </summary>
    /// <param name="user">L'utente di cui creare il ViewModel.</param>
    /// <param name="primaryRole">Ruolo principale assegnato all'utente.</param>
    public static UserViewModel FromApplicationUser(ApplicationUser user, string primaryRole)
    {
        return new UserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            UserName = user.UserName!,
            Email = user.Email!,
            Role = primaryRole,
            IsActive = user.IsActive,
            RegistrationDate = user.RegistrationDate,
            LastLoginDate = user.LastLoginDate,
            DeactivationDate = user.DeactivationDate,
            DeactivationReason = user.DeactivationReason,
            DeactivateModalViewModel = new DeactivateUserModalViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                IsCurrentlyActive = user.IsActive
            }
        };
    }

    /// <summary>
    ///     Aggiorna le proprietà di un ApplicationUser a partire dal ViewModel.
    /// </summary>
    /// <param name="user">L'utente da aggiornare.</param>
    public void UpdateApplicationUser(ApplicationUser user)
    {
        user.FullName = FullName;
        user.Email = Email;
        user.IsActive = IsActive;
        user.DeactivationDate = DeactivationDate;
        user.DeactivationReason = DeactivationReason;
    }
}