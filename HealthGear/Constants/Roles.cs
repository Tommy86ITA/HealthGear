namespace HealthGear.Constants;

/// <summary>
///     Contiene le costanti per i ruoli utilizzati in HealthGear.
/// </summary>
public static class Roles
{
    /// <summary>
    ///     Admin: Controllo completo su dispositivi, interventi, files e gestione degli account.
    ///     È l'unico ruolo con accesso alle funzionalità di amministrazione avanzata (es. cleanup dei file).
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    ///     Tecnico: Può aggiungere, visualizzare e modificare dispositivi, ma non dismetterli né eliminarli.
    ///     Può gestire aggiungere e modificare gli interventi, ma non eliminarli
    ///     Può gestire liberamente i files
    /// </summary>
    public const string Tecnico = "Tecnico";

    /// <summary>
    ///     Office: Ruolo con restrizioni maggiori, accessibile solo in modalità sola lettura per dispositivi, interventi e
    ///     files.
    /// </summary>
    public const string Office = "Office";
}