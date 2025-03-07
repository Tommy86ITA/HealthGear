using System.ComponentModel.DataAnnotations;

namespace HealthGear.Models.ViewModels;

public class EditProfileViewModel
{
    [Required]
    [Display(Name = "Nome Completo")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Numero di Telefono")]
    public string? PhoneNumber { get; set; }
}