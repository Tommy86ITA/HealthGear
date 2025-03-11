using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models.ViewModels;

public class FirstLoginViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "La password è obbligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "La password deve avere almeno {2} caratteri.", MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Conferma la password.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Le password non coincidono.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}