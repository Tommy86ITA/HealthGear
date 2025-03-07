using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models.ViewModels;

/// <summary>
///     Modello per il reset della password.
/// </summary>
public class ResetPasswordViewModel
{
    [Required] public string Email { get; set; } = string.Empty;

    [Required] public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nuova password è obbligatoria.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Conferma la nuova password.")]
    [Compare("NewPassword", ErrorMessage = "Le password non coincidono.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}