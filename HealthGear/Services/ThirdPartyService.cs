using System.Text.Json;
using HealthGear.Models;

namespace HealthGear.Services;

/// <summary>
///     Servizio per la gestione dei componenti di terze parti.
/// </summary>
public class ThirdPartyService
{
    private const string FileName = "thirdparty-license.json";
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    ///     Inizializza una nuova istanza del <see cref="ThirdPartyService" />.
    /// </summary>
    /// <param name="environment">Contesto dell'ambiente di esecuzione.</param>
    public ThirdPartyService(IWebHostEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    ///     Restituisce l'elenco dei componenti di terze parti utilizzati nel progetto.
    ///     I dati vengono caricati dinamicamente da un file JSON.
    /// </summary>
    /// <returns>Lista di <see cref="ThirdPartyComponent" />.</returns>
    public List<ThirdPartyComponent> GetThirdPartyComponents()
    {
        var filePath = Path.Combine(_environment.ContentRootPath, FileName);

        if (!File.Exists(filePath))
            return
            [
                new ThirdPartyComponent
                {
                    Name = "Dati non disponibili",
                    Version = "N/A",
                    License = "N/A"
                }
            ];

        var json = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<List<ThirdPartyComponent>>(json) ?? [];
    }
}