namespace HealthGear.Constants;

/// <summary>
/// Contiene le costanti per i ruoli utilizzati in HealthGear.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Admin: Controllo completo su dispositivi, interventi e gestione degli account.
    /// È l'unico ruolo con accesso alle funzionalità di amministrazione avanzata (es. cleanup dei file).
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Tecnico: Può aggiungere, visualizzare e modificare dispositivi, ma non dismetterli né eliminarli.
    /// Può gestire liberamente gli interventi.
    /// </summary>
    public const string Tecnico = "Tecnico";

    /// <summary>
    /// Office: Ruolo con restrizioni maggiori, accessibile solo in modalità sola lettura per dispositivi e interventi.
    /// </summary>
    public const string Office = "Office";
}