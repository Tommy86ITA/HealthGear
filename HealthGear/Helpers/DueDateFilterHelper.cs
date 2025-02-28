namespace HealthGear.Helpers;

public static class DueDateFilterHelper
{
    /// <summary>
    ///     Restituisce true se la data di scadenza è già passata o rientra nei prossimi 2 mesi.
    /// </summary>
    /// <param name="dueDate">La data di scadenza da verificare</param>
    /// <returns>true se la data è scaduta o in scadenza entro 2 mesi, altrimenti false</returns>
    public static bool IsDueSoonOrExpired(DateTime? dueDate)
    {
        if (!dueDate.HasValue)
            return false; // oppure true, a seconda di come vuoi gestire i casi senza data

        var today = DateTime.Today;
        var threshold = today.AddMonths(2);
        return dueDate.Value < threshold;
    }
}