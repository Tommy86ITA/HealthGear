using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGear.Models;

public class PhysicalInspection
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Il dispositivo è obbligatorio.")]
    public int DeviceId { get; set; }

    [ForeignKey("DeviceId")] public Device? Device { get; set; }

    [Required(ErrorMessage = "La data della verifica è obbligatoria.")]
    [DataType(DataType.Date)]
    public DateTime InspectionDate { get; set; }

    [Required(ErrorMessage = "Il nome dell'Esperto Qualificato è obbligatorio.")]
    [MinLength(3, ErrorMessage = "Inserisci almeno 3 caratteri.")]
    public string PerformedBy { get; set; }

    [Required(ErrorMessage = "L'esito della verifica è obbligatorio.")]
    public bool Passed { get; set; } // True = Superato, False = Non Superato

    public string? Notes { get; set; }

    public virtual ICollection<FileDocument> Documents { get; set; } = new List<FileDocument>();

    // Metodo per calcolare la prossima scadenza in base alla tipologia del dispositivo
    public DateTime CalculateNextInspectionDate()
    {
        if (Device != null && Device.DeviceType.ToLower() == "radiogeno")
        {
            if (Device.Model.ToLower().Contains("mammografo")) return InspectionDate.AddMonths(6);
            return InspectionDate.AddYears(1);
        }

        throw new InvalidOperationException("Il controllo fisico è applicabile solo ad apparecchiature radiogene.");
    }
}