// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace HealthGear.Models.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}