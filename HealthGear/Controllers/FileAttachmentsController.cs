using HealthGear.Constants;
using HealthGear.Data;
using HealthGear.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Controllers;

public class FileAttachmentsController(
    ApplicationDbContext context,
    IWebHostEnvironment env,
    ICompositeViewEngine viewEngine)
    : Controller
{
    // Cartella in wwwroot dove verranno salvati i file
    private const string UploadsFolder = "uploads";

    // Limite massimo di dimensione dei file (500 MB)
    private const long MaxFileSize = 500 * 1024 * 1024;

    // Estensioni consentite
    private readonly string[] _allowedExtensions =
    [
        ".pdf", ".bmp", ".jpg", ".jpeg", ".png", ".doc", ".docx",
        ".xls", ".xlsx", ".txt", ".ini", ".cfg", ".json", ".dcm",
        ".zip", ".rar"
    ];

    // I campi context, env e viewEngine vengono passati tramite dependency injection

    /// <summary>
    ///     AjaxUpload: Carica i file inviati via AJAX.
    ///     Per ogni file valido (dimensione, estensione) viene salvato fisicamente
    ///     e viene creato un record FileAttachment associato a DeviceId e/o InterventionId.
    ///     Al termine, viene renderizzata la partial che mostra i file allegati.
    /// </summary>
    /// <param name="deviceId">ID del dispositivo (se presente)</param>
    /// <param name="interventionId">ID dell'intervento (se presente)</param>
    /// <param name="files">Lista dei file inviati</param>
    /// <param name="documentType">Tipo di documento (stringa generica)</param>
    /// <returns>JSON con success = true e il markup aggiornato della partial oppure un messaggio di errore</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> AjaxUpload(int? deviceId, int? interventionId, List<IFormFile>? files,
        string documentType)
    {
        try
        {
            // Se non sono stati inviati file, restituisce un errore JSON
            if (files == null || files.Count == 0)
                return Json(new { success = false, errorMessage = "Nessun file selezionato." });

            // Costruiamo il percorso per il salvataggio dei file (wwwroot/uploads)
            var subFolder = deviceId.HasValue ? "devices" : interventionId.HasValue ? "interventions" : "misc";
            var uploadsPath = Path.Combine(env.WebRootPath, UploadsFolder, subFolder);
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Itera su ogni file inviato che ha una lunghezza > 0
            foreach (var file in files.Where(f => f.Length > 0))
            {
                // Verifica che il file non superi il limite massimo
                if (file.Length > MaxFileSize)
                    continue;

                // Verifica l'estensione del file
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || Array.IndexOf(_allowedExtensions, ext) < 0)
                    continue;

                // Genera un nome file univoco (usando Guid)
                var uniqueFileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Salva il file fisico nel percorso definito
                try
                {
                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                }
                catch (Exception exFile)
                {
                    return Json(new
                    {
                        success = false,
                        errorMessage = $"Errore durante il caricamento di {file.FileName}: {exFile.Message}"
                    });
                }

                // Crea il record FileAttachment con le informazioni del file
                var attachment = new FileAttachment
                {
                    FileName = file.FileName,
                    FilePath = "/" + UploadsFolder + "/" + subFolder + "/" + uniqueFileName,
                    ContentType = file.ContentType,
                    UploadDate = DateTime.UtcNow,
                    DocumentType = documentType, // Salva il tipo documento come stringa
                    DeviceId = deviceId,
                    InterventionId = interventionId
                };

                context.FileAttachments.Add(attachment);
            }

            // Se il ModelState non è valido, restituisce un errore JSON
            if (!ModelState.IsValid)
                return Json(new { success = false, errorMessage = "Errore nel caricamento dei file." });

            // Salva i record creati nel database
            await context.SaveChangesAsync();

            // Recupera la lista aggiornata degli allegati in base al contesto
            IEnumerable<FileAttachment> updatedAttachments;
            if (deviceId.HasValue)
                updatedAttachments = await context.FileAttachments
                    .Where(f => f.DeviceId == deviceId.Value)
                    .ToListAsync();
            else if (interventionId.HasValue)
                updatedAttachments = await context.FileAttachments
                    .Where(f => f.InterventionId == interventionId.Value)
                    .ToListAsync();
            else
                updatedAttachments = Array.Empty<FileAttachment>();

            // Costruisce un modello anonimo con le proprietà attese dalla partial
            var model = new
            {
                DeviceId = deviceId,
                InterventionId = interventionId,
                FileAttachments = updatedAttachments
            };

            // Renderizza la partial "_FileUploadPartial" in una stringa HTML
            var html = await RenderPartialViewToString("_FileUploadPartial", model);

            // Restituisce un JSON con successo e l'HTML aggiornato
            return Json(new { success = true, html });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, errorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Download: scarica un file allegato.
    ///     Cerca il record nel database, scarica il file fisico se presente,
    ///     e se non è presente invia alla pagina 404.
    /// </summary>
    /// <param name="id">ID dell'allegato da scaricare</param>
    /// <returns>File allegato o redirect a pagina 404</returns>
    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico + "," + Roles.Office)]
    public async Task<IActionResult> Download(int id)
    {
        // Recupera il record dell'allegato dal database in base all'ID fornito.
        var attachment = await context.FileAttachments.FindAsync(id);
        if (attachment == null)
            // Se il record non esiste, restituisce un NotFound.
            return NotFound("Il file richiesto non è stato trovato nel database.");

        // Costruisce il percorso fisico del file combinando la root del sito con il percorso salvato,
        // rimuovendo eventuali '/' iniziali.
        var filePath = Path.Combine(env.WebRootPath, attachment.FilePath.TrimStart('/'));

        // Verifica se il file esiste sul filesystem.
        if (System.IO.File.Exists(filePath)) return PhysicalFile(filePath, attachment.ContentType, attachment.FileName);
        // Se il file non esiste, imposta un messaggio in ViewBag e restituisce la view "FileNotAvailable".
        ViewBag.ErrorMessage = "Il file non è presente sul server. Potrebbe essere stato rimosso manualmente.";
        return View("FileNotAvailable");

        // Se tutto è a posto, restituisce il file fisico per il download.
    }


    /// <summary>
    ///     Delete: Elimina un file allegato.
    ///     Cerca il record nel database, elimina il file fisico se presente,
    ///     rimuove il record e restituisce un JSON con success = true oppure un messaggio d'errore.
    /// </summary>
    /// <param name="id">ID dell'allegato da eliminare</param>
    /// <returns>JSON con success = true o un messaggio di errore</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Tecnico)]
    public async Task<IActionResult> Delete(int id)
    {
        // Trova il record dell'allegato
        var attachment = await context.FileAttachments.FindAsync(id);
        if (attachment == null)
            return Json(new { success = false, errorMessage = "File non trovato." });

        // Costruisce il percorso fisico del file (rimuovendo eventuali '/' iniziali)
        var filePath = Path.Combine(env.WebRootPath, attachment.FilePath.TrimStart('/'));
        try
        {
            // Se il file esiste, lo elimina dal filesystem
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            else
                await Console.Error.WriteLineAsync(
                    $"Warning: Il file '{filePath}' non esiste, ma il record verrà rimosso.");
        }
        catch (Exception ex)
        {
            return Json(new
                { success = false, errorMessage = $"Errore durante l'eliminazione del file: {ex.Message}" });
        }

        // Rimuove il record dal database
        context.FileAttachments.Remove(attachment);
        await context.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    ///     RenderPartialViewToString: Helper per rendere una partial view come stringa.
    ///     Utile per aggiornamenti via AJAX.
    /// </summary>
    /// <param name="viewName">Nome della partial view</param>
    /// <param name="model">Modello da passare alla partial</param>
    /// <returns>Stringa HTML renderizzata</returns>
    private async Task<string> RenderPartialViewToString(string viewName, object model)
    {
        // Imposta il modello nei ViewData
        ViewData.Model = model;

        await using var sw = new StringWriter();
        var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

        if (viewResult.View == null)
            throw new ArgumentNullException($"{viewName} non trovato.");

        var viewContext = new ViewContext(
            ControllerContext,
            viewResult.View,
            ViewData,
            TempData,
            sw,
            new HtmlHelperOptions()
        );

        // Renderizza la view nel StringWriter e restituisce la stringa HTML risultante
        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }


    /// <summary>
    ///     CleanupOrphanedFiles
    ///     Pulisce il database dai file rimossi dal file system ma ancora registrati nel DB.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CleanupOrphanedFilesAjax()
    {
        // Recupera tutti i record di file allegati dal database
        var allAttachments = await context.FileAttachments.ToListAsync();
        var removedCount = 0;

        // Per ogni record, controlla se il file esiste nel filesystem
        foreach (var attachment in from attachment in allAttachments
                 let filePath = Path.Combine(env.WebRootPath, attachment.FilePath.TrimStart('/'))
                 where !System.IO.File.Exists(filePath)
                 select attachment)
        {
            // Se il file non esiste, rimuovi il record
            context.FileAttachments.Remove(attachment);
            removedCount++;
        }

        // Salva le modifiche al database
        await context.SaveChangesAsync();

        // Restituisce il report in formato JSON
        return Json(new { totalRecords = allAttachments.Count, removedCount });
    }
}