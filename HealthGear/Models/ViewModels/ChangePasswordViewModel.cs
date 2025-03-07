using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models.ViewModels;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Le password non coincidono.")]
    public string ConfirmPassword { get; set; } = null!;
}