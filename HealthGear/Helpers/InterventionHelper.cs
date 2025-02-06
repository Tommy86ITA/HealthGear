#region

using HealthGear.Models;

#endregion

namespace HealthGear.Helpers;

public static class InterventionHelper
{
    public static string GetInterventionDisplayName(Intervention intervention)
    {
        return intervention.Type switch
        {
            InterventionType.ElectricalTest => "Verifica Elettrica",
            InterventionType.PhysicalInspection => "Verifica Fisica",
            InterventionType.Maintenance => intervention.MaintenanceCategory switch
            {
                MaintenanceType.Preventive => "Manutenzione - Preventiva",
                MaintenanceType.Corrective => "Manutenzione - Correttiva",
                _ => "Manutenzione - (Tipo sconosciuto)" // Gestione fallback per i vecchi valori
            },
            _ => "Intervento - (Tipo sconosciuto)"
        };
    }

    public static string GetInterventionIcon(Intervention intervention)
    {
        return intervention.Type switch
        {
            InterventionType.Maintenance => "<i class='fas fa-wrench'></i>", // 🔧 Icona chiave inglese
            InterventionType.ElectricalTest => "<i class='fas fa-bolt'></i>", // ⚡ Icona fulmine
            InterventionType.PhysicalInspection => "<i class='fas fa-radiation'></i>", // ☢ Icona radiazioni
            _ => "<i class='fas fa-question-circle'></i>" // ❓ Icona di default per interventi sconosciuti
        };
    }
}