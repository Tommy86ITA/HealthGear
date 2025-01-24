using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class ElectricalTest
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Il dispositivo è obbligatorio.")]
    public int DeviceId { get; set; }

    [ForeignKey("DeviceId")]
    public Device? Device { get; set; }

    [Required(ErrorMessage = "La data della verifica è obbligatoria.")]
    [DataType(DataType.Date)]
    public DateTime TestDate { get; set; }

    [Required(ErrorMessage = "Il nome del tecnico è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome del tecnico non può superare i 100 caratteri.")]
    public string PerformedBy { get; set; }

    [Required(ErrorMessage = "L'esito della verifica è obbligatorio.")]
    public bool Passed { get; set; }  // True = Superato, False = Non Superato

    public string? Notes { get; set; }

    public ICollection<ElectricalTestDocument> Documents { get; set; } = new List<ElectricalTestDocument>();
}