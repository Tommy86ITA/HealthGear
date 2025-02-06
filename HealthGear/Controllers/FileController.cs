#region

using HealthGear.Services;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace HealthGear.Controllers;

[Route("File")]
public class FileController : Controller
{
    private readonly FileService _fileService;
    private readonly ILogger<FileController> _logger;

    public FileController(FileService fileService, ILogger<FileController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    ///     API per il caricamento dei file, delegata a FileService.
    /// </summary>
    [HttpPost("Upload")]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            _logger.LogWarning("⚠️ Nessun file selezionato per l'upload.");
            return Json(new { success = false, message = "Nessun file selezionato." });
        }

        try
        {
            var savedFiles =
                await _fileService.SaveFilesAsync(files, 0, "Generic", "Unknown"); // Cambia questi valori se necessario
            return Json(new
                { success = true, message = $"{savedFiles.Count} file caricati con successo.", files = savedFiles });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Errore durante il caricamento dei file.");
            return StatusCode(500, $"Errore interno del server: {ex.Message}");
        }
    }

    /// <summary>
    ///     API per eliminare un file specifico, delegata a FileService.
    /// </summary>
    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Json(new { success = false, message = "Nome del file non valido." });

        var success = await _fileService.DeleteFileAsync(filePath);
        return Json(success
            ? new { success = true, message = $"Il file {filePath} è stato eliminato." }
            : new { success = false, message = "Errore durante l'eliminazione del file." });
    }

    /// <summary>
    ///     API per recuperare la lista dei file associati a un intervento specifico.
    /// </summary>
    [HttpGet("List")]
    public IActionResult ListFiles(string interventionType, string deviceName)
    {
        if (string.IsNullOrWhiteSpace(interventionType) || string.IsNullOrWhiteSpace(deviceName))
            return Json(new { success = false, message = "Intervento o nome dispositivo non validi." });

        var files = _fileService.ListFiles(interventionType, deviceName);
        return files.Count != 0
            ? Json(new { success = true, files })
            : Json(new { success = false, message = "Nessun file trovato." });
    }
}