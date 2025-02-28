// File: ScriptRegistrar.cs
// Questo helper consente di registrare e renderizzare blocchi script da partial view.
// I blocchi script vengono salvati temporaneamente in HttpContext.Items e poi concatenati nel layout.

using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HealthGear.Helpers;

public static class ScriptRegistrar
{
    // Chiave usata per salvare i blocchi script in HttpContext.Items
    private const string ScriptBlocksKey = "RegisteredScriptBlocks";

    // Metodo di estensione per registrare un blocco script
    // htmlHelper: contesto HTML corrente
    // scriptBlock: stringa contenente il blocco script (inclusi i tag <script>)
    public static void RegisterScriptBlock(this IHtmlHelper htmlHelper, string scriptBlock)
    {
        // Recupera la lista dei blocchi script dal contesto, se già presente

        // Se la lista non esiste, la creiamo e la assegniamo al contesto
        if (htmlHelper.ViewContext.HttpContext.Items[ScriptBlocksKey] is not List<string> scriptBlocks)
        {
            scriptBlocks = [];
            htmlHelper.ViewContext.HttpContext.Items[ScriptBlocksKey] = scriptBlocks;
        }

        // Aggiunge il blocco script alla lista
        scriptBlocks.Add(scriptBlock);
    }

    // Metodo di estensione per renderizzare tutti i blocchi script registrati
    // htmlHelper: contesto HTML corrente
    // Restituisce un IHtmlContent contenente i blocchi script concatenati
    public static IHtmlContent RenderScriptBlocks(this IHtmlHelper htmlHelper)
    {
        // Recupera la lista dei blocchi script dal contesto

        // Se non ci sono script registrati, restituisce un contenuto HTML vuoto
        if (htmlHelper.ViewContext.HttpContext.Items[ScriptBlocksKey] is not List<string> scriptBlocks ||
            scriptBlocks.Count == 0) return HtmlString.Empty;

        // Utilizza StringBuilder per concatenare tutti i blocchi script
        var sb = new StringBuilder();
        foreach (var script in scriptBlocks) sb.AppendLine(script);

        // Restituisce il contenuto HTML contenente i blocchi script concatenati
        return new HtmlString(sb.ToString());
    }
}