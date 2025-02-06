#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace HealthGear.Models;

public class FileDocument
{
    [Key] public int Id { get; init; }

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    [Required] public int ParentEntityId { get; set; } // ID dell'intervento associato

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    [Required] [MaxLength(255)] public string FileName { get; set; } = string.Empty;

    [Required] [MaxLength(500)] public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string InterventionType { get; set; } = string.Empty; // "Maintenance", "ElectricalTest", etc.

    [Required] [MaxLength(255)] public string DeviceName { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.Now;
}