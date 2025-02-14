using HealthGear.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthGear.Services;

/// <summary>
/// Servizio per la generazione automatica dei numeri di inventario.
/// </summary>
public class InventoryNumberService(ApplicationDbContext context)
{
    /// <summary>
    /// Genera un numero di inventario univoco basato sull'anno di collaudo.
    /// </summary>
    /// <param name="year">Anno di collaudo del dispositivo</param>
    /// <returns>Numero di inventario formattato (es. "2024-001")</returns>
    public async Task<string> GenerateInventoryNumberAsync(int year)
    {
        // Conta quanti dispositivi sono già registrati con lo stesso anno di collaudo
        int count = await context.Devices
            .Where(d => d.DataCollaudo.Year == year)
            .CountAsync() + 1;

        // Formatta il numero con tre cifre (es. "2024-001", "2024-002")
        return $"{year}-{count:D3}";
    }
}