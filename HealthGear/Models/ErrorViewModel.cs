// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace HealthGear.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}