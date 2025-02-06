#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace HealthGear.Models;

public class ConfirmDeleteModel
{
    [Required(ErrorMessage = "Devi inserire il nome esatto per confermare l'eliminazione.")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string ConfirmName { get; set; }
}