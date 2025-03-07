using System.Diagnostics;

namespace HealthGear.Models.ViewModels;

/// <summary>
///     ViewModel per la pagina About, che fornisce informazioni sulla versione
///     dell'applicazione, la data di build e l'elenco dei componenti di terze parti utilizzati.
/// </summary>
public class AboutViewModel
{
    /// <summary>
    ///     Versione attuale dell'applicazione, recuperata automaticamente da GitVersion.
    /// </summary>
    public string Versione { get; set; } = GetGitVersion();

    /// <summary>
    ///     Data di build dell'applicazione.
    /// </summary>
    public DateTime DataBuild { get; set; }

    /// <summary>
    ///     Elenco dei componenti di terze parti utilizzati nel progetto, con le relative licenze.
    /// </summary>
    public List<ThirdPartyComponent> ThirdPartyComponents { get; set; } = [];

    /// <summary>
    ///     Recupera il numero di versione dell'applicazione utilizzando GitVersion.
    /// </summary>
    /// <returns>Il numero di versione in formato SemVer, oppure "Versione non disponibile" in caso di errore.</returns>
    private static string GetGitVersion()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "gitversion /showvariable SemVer",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return output;
        }
        catch
        {
            return "Versione non disponibile";
        }
    }
}