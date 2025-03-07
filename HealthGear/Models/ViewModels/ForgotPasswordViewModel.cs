using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models.ViewModels;

/// <summary>
///     Modello per la richiesta di reset della password.
/// </summary>
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "L'email è obbligatoria.")]
    [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido.")]
    public string Email { get; set; } = string.Empty;
}