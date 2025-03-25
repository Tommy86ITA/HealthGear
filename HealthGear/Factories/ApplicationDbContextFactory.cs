using HealthGear.Data;
using HealthGear.Models.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace HealthGear.Factories
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Percorso fisso al file di configurazione
            var configPath = Path.Combine(
                @"C:\ProgramData\HealthGear Suite\HealthGearConfig", "healthgear.config.json");

            HealthGearConfig config = HealthGearConfig.CreateDefault();

            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    config = JsonSerializer.Deserialize<HealthGearConfig>(json) ?? HealthGearConfig.CreateDefault();
                    Console.WriteLine("✅ Configurazione letta correttamente dalla factory.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Errore nella lettura del file di configurazione:");
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("⚠️ File di configurazione non trovato. Uso dei valori di default.");
            }

            optionsBuilder.UseSqlite($"Data Source={config.DatabasePath}");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}