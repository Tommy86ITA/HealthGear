using QRCoder;

namespace HealthGear.Services;

/// <summary>
///     Servizio per la generazione di QR Code per i dispositivi.
/// </summary>
public class QrCodeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///     Costruttore del servizio QrCodeService.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor per ottenere il contesto HTTP.</param>
    public QrCodeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    ///     Genera un QR Code per l'URL di un dispositivo, costruendo dinamicamente l'URL.
    /// </summary>
    /// <param name="deviceId">L'ID del dispositivo.</param>
    /// <returns>Un array di byte contenente l'immagine del QR Code in formato PNG.</returns>
    public byte[] GenerateQrCode(int deviceId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext non disponibile.");

        var localIp = httpContext.Connection.LocalIpAddress?.ToString() ?? "localhost";

        // Se l'IP è "::1" (IPv6 per localhost), convertirlo in "127.0.0.1"
        if (localIp == "::1") localIp = "127.0.0.1";

        var url = $"{httpContext.Request.Scheme}://{localIp}:{httpContext.Request.Host.Port}/Device/Details/{deviceId}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }
}