using HealthGear.Data;
using HealthGear.Models.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace HealthGear.Factories
{
    public class SettingsDbContextFactory : IDesignTimeDbContextFactory<SettingsDbContext>
    {
        public SettingsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SettingsDbContext>();

            // Percorso fisso alla configurazione condivisa
            var configPath = Path.Combine(
                @"C:\ProgramData\HealthGear Suite\HealthGearConfig", "healthgear.config.json");

            HealthGearConfig config = HealthGearConfig.CreateDefault();

            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    config = JsonSerializer.Deserialize<HealthGearConfig>(json) ?? HealthGearConfig.CreateDefault();
                    Console.WriteLine("✅ Configurazione letta correttamente per SettingsDbContext.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Errore nella lettura del file di configurazione (SettingsDbContext):");
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("⚠️ File di configurazione non trovato. Uso dei valori di default (SettingsDbContext).");
            }

            optionsBuilder.UseSqlite($"Data Source={config.SettingsDbPath}");

            return new SettingsDbContext(optionsBuilder.Options);
        }
    }
}