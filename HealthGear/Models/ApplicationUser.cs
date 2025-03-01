using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HealthGear.Models;

/// <summary>
/// Rappresenta l'utente dell'applicazione. Estende IdentityUser per integrare il sistema di autenticazione di ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // FullName: Il nome completo dell'utente.
    // L'attributo [Required] impone che venga specificato e [MaxLength(100)] limita la lunghezza a 100 caratteri.
    // Il modificatore "required" (C# 11) indica che questa proprietà deve essere inizializzata.
    [Required(ErrorMessage = "Il nome completo è obbligatorio.")]
    [MaxLength(100, ErrorMessage = "Il nome completo non può superare 100 caratteri.")]
    public required string FullName { get; set; }

    // IsActive: Indica se l'account dell'utente è attivo.
    // Anche se il tipo bool è non-nullable, l'attributo [Required] serve per enfatizzare che questo campo è fondamentale.
    [Required]
    public bool IsActive { get; set; }

    // RegistrationDate: La data in cui l'utente si è registrato.
    public DateTime RegistrationDate { get; set; }

    // LastLoginDate: La data dell'ultimo accesso. Può essere null se l'utente non ha mai effettuato il login.
    public DateTime? LastLoginDate { get; set; }
}