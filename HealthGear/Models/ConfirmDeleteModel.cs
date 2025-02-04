#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace HealthGear.Models;

public class ConfirmDeleteModel
{
    [Required(ErrorMessage = "Devi inserire il nome esatto per confermare l'eliminazione.")]
    public required string ConfirmName { get; set; }
}