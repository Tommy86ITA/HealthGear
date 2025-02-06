#region

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

#endregion

namespace HealthGear.Helpers;

/// <summary>
///     Estende la classe Controller con un metodo per renderizzare una vista in formato stringa.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    ///     Converte una vista Razor in una stringa HTML.
    /// </summary>
    /// <param name="controller">L'istanza del controller chiamante.</param>
    /// <param name="viewName">Il nome della vista da renderizzare.</param>
    /// <param name="model">Il modello da passare alla vista.</param>
    /// <returns>Una stringa contenente il codice HTML renderizzato.</returns>
    public static async Task<string> RenderViewToStringAsync(this Controller controller, string viewName, object model)
    {
        if (controller == null)
            throw new ArgumentNullException(nameof(controller), "Il controller non può essere null.");

        if (string.IsNullOrEmpty(viewName))
            throw new ArgumentException("Il nome della vista non può essere nullo o vuoto.", nameof(viewName));

        // 🔹 Impostiamo il modello nei dati della vista
        controller.ViewData.Model = model;

        // 🔹 Creiamo un oggetto StringWriter per raccogliere l'output HTML della vista
        await using var writer = new StringWriter();

        // Recuperiamo il motore di rendering delle viste

        if (controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) is not ICompositeViewEngine
            viewEngine)
            throw new InvalidOperationException("Impossibile ottenere un'istanza valida di ICompositeViewEngine.");

        // Cerchiamo la vista richiesta
        var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

        // ✅ Se la vista non esiste, lanciamo un'eccezione con un messaggio chiaro
        if (viewResult.View == null)
            throw new InvalidOperationException(
                $"La vista '{viewName}' non è stata trovata. Assicurati che il nome sia corretto e che la vista esista.");

        var view = viewResult.View;

        // 🔹 Creiamo il contesto di rendering della vista
        var viewContext = new ViewContext(
            controller.ControllerContext,
            view,
            controller.ViewData,
            controller.TempData,
            writer,
            new HtmlHelperOptions()
        );

        // 🔹 Renderizziamo la vista e scriviamo il risultato nel writer
        await view.RenderAsync(viewContext);

        // 🔹 Restituiamo la stringa HTML generata
        return writer.ToString();
    }
}