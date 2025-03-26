using System.Diagnostics;
using System.Reflection;

namespace HealthGear.Models.ViewModels;

/// <summary>
///     ViewModel per la pagina About, che fornisce informazioni sulla versione
///     dell'applicazione, la data di build e l'elenco dei componenti di terze parti utilizzati.
/// </summary>
public class AboutViewModel
{
    /// <summary>
    ///     Versione semantica completa (es. 1.2.0-preview.3).
    /// </summary>
    public string Versione { get; set; }

    /// <summary>
    ///     Tag della versione (es. 1.2.0-preview.3), equivalente a Versione.
    /// </summary>
    public string TagVersione { get; set; }

    /// <summary>
    ///     Canale della release (es. preview, beta, rc, stable).
    /// </summary>
    public string Canale { get; set; }

    /// <summary>
    ///     Hash completo del commit corrente.
    /// </summary>
    public string Build { get; set; }

    /// <summary>
    ///     Hash breve del commit corrente (per visualizzazione compatta).
    /// </summary>
    public string ShortBuild => Build.Length >= 7 ? Build[..7] : Build;

    /// <summary>
    ///     Data di build dell'applicazione.
    /// </summary>
    public DateTime DataBuild { get; set; }

    /// <summary>
    ///     Elenco dei componenti di terze parti utilizzati nel progetto, con le relative licenze.
    /// </summary>
    public List<ThirdPartyComponent> ThirdPartyComponents { get; set; } = [];

    public AboutViewModel()
    {
        var versionInfo = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Versione non disponibile";

        // Esempio: "1.2.0-preview.3+abc123def456"
        var parts = versionInfo.Split('+');
        TagVersione = parts[0]; // 1.2.0-preview.3
        Versione = TagVersione;
        Build = parts.Length > 1 ? parts[1] : "N/A";

        // Canale (preview, beta, rc, stable)
        Canale = Versione.Contains('-')
            ? Versione.Split('-')[1].Split('.')[0]
            : "stable";

        DataBuild = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
    }
}