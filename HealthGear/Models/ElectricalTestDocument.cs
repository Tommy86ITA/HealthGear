using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class ElectricalTestDocument
{
    [Key] public int Id { get; set; }

    [Required] public int ElectricalTestId { get; set; }

    [ForeignKey("ElectricalTestId")] public ElectricalTest ElectricalTest { get; set; }

    [Required] public string FileName { get; set; }

    [Required] public string FilePath { get; set; }
}