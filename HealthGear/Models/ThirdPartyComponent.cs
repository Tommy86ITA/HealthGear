namespace HealthGear.Models;

/// <summary>
///     Rappresenta un componente di terze parti utilizzato nel progetto.
/// </summary>
public class ThirdPartyComponent
{
    /// <summary>
    ///     Nome del componente.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Versione utilizzata.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    ///     Licenza del componente.
    /// </summary>
    public string License { get; set; } = string.Empty;
}