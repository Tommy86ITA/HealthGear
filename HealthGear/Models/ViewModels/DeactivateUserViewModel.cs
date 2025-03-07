namespace HealthGear.Models.ViewModels;

/// <summary>
///     Modello per la partial di disattivazione/riattivazione utente.
///     Questo modello viene utilizzato per passare i dati necessari al modal di conferma disattivazione/riattivazione.
/// </summary>
public class DeactivateUserModalViewModel
{
    /// <summary>
    ///     ID univoco dell'utente da disattivare o riattivare.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    ///     Nome completo dell'utente, mostrato nel testo informativo del modal.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    ///     Indica se l'utente è attualmente attivo.
    ///     Questo valore viene utilizzato per determinare se il modal deve mostrare la conferma di disattivazione o di
    ///     riattivazione.
    /// </summary>
    public required bool IsCurrentlyActive { get; set; }
}