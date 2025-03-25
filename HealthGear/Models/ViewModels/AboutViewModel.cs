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
    ///     Versione in formato semplificato (es. 1.0.0).
    /// </summary>
    public string Versione { get; set; }

    /// <summary>
    ///     Tag della versione completo (es. 1.0.0-preview.1).
    /// </summary>
    public string TagVersione { get; set; }

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

        // Es. "1.0.0-preview.1+e7ab1195e46..."
        var parts = versionInfo.Split('+'); // es. 1.0.0-preview+e7ab119
        Versione = parts[0]; // 1.0.0-preview
        Build = parts.Length > 1 ? parts[1] : "N/A";

        DataBuild = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
    }
}