using HealthGear.Models;

namespace HealthGear.Services;

public class FileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    ///     Salva i file nella cartella dell'intervento specifico.
    /// </summary>
    public async Task<List<FileDocument>> SaveFilesAsync(List<IFormFile> files, int parentEntityId,
        string interventionType, string deviceName)
    {
        var savedFiles = new List<FileDocument>();
        var sanitizedDeviceName = SanitizeFolderName(deviceName);
        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", interventionType, sanitizedDeviceName);

        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
            _logger.LogInformation("📁 Cartella creata: {UploadsPath}", uploadsPath);
        }

        foreach (var file in files)
            try
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var fileDocument = new FileDocument
                {
                    ParentEntityId = parentEntityId,
                    FileName = fileName,
                    FilePath = $"/uploads/{interventionType}/{sanitizedDeviceName}/{fileName}",
                    InterventionType = interventionType,
                    DeviceName = deviceName,
                    UploadedAt = DateTime.Now
                };

                savedFiles.Add(fileDocument);
                _logger.LogInformation("✅ File salvato: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Errore durante il salvataggio del file.");
            }

        return savedFiles;
    }

    /// <summary>
    ///     Elimina un file dal server.
    /// </summary>
    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("🗑️ File eliminato: {FilePath}", fullPath);
                return await Task.FromResult(true);
            }

            _logger.LogWarning("⚠️ File non trovato: {FilePath}", fullPath);
            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Errore durante l'eliminazione del file: {FilePath}", filePath);
            return await Task.FromResult(false);
        }
    }

    /// <summary>
    ///     Recupera la lista dei file di un intervento.
    /// </summary>
    public List<string> ListFiles(string interventionType, string deviceName)
    {
        var sanitizedDeviceName = SanitizeFolderName(deviceName);
        var directoryPath = Path.Combine(_environment.WebRootPath, "uploads", interventionType, sanitizedDeviceName);

        if (Directory.Exists(directoryPath))
            return Directory.GetFiles(directoryPath)
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Select(f => f!)
                .ToList();
        _logger.LogWarning("⚠️ La cartella non esiste: {DirectoryPath}", directoryPath);
        return [];
    }

    /// <summary>
    ///     Rimuove caratteri non validi dal nome della cartella.
    /// </summary>
    private string SanitizeFolderName(string name)
    {
        return name.Replace(" ", "_").Replace("/", "-");
    }
}