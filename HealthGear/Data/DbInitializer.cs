using HealthGear.Constants;
using HealthGear.Models;
using HealthGear.Models.Config;
using HealthGear.Services;
using Microsoft.AspNetCore.Identity;

namespace HealthGear.Data;

public class AdminConfig
{
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
}

public static class DbInitializer
{
    /// <summary>
    ///     Esegue il seeding iniziale dei ruoli e dell'utente admin, usando User Secrets per recuperare i dati sensibili.
    /// </summary>
    public static async Task SeedDataAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        await SeedSettingsAsync(serviceProvider);
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        Console.WriteLine("🏁 Avvio seeding ruoli e utente admin di default...");

        try
        {
            string[] roleNames = [Roles.Admin, Roles.Tecnico, Roles.Office];

            foreach (var roleName in roleNames)
            {
                if (await RoleExists(roleManager, roleName)) continue;

                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine(result.Succeeded
                    ? $"✅ Ruolo creato: {roleName}"
                    : $"❌ Errore nella creazione del ruolo {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var adminConfig = new AdminConfig
            {
                UserName = "Admin",
                FullName = "Administrator",
                Email = "admin.default@healthgear.local",
                Role = Roles.Admin,
                Password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Password123!"
            };

            var adminUser = await userManager.FindByEmailAsync(adminConfig.Email);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminConfig.UserName,
                    FullName = adminConfig.FullName,
                    Email = adminConfig.Email,
                    IsActive = true,
                    RegistrationDate = DateTime.UtcNow,
                    EmailConfirmed = true,
                    MustChangePassword = false
                };

                var createAdmin = await userManager.CreateAsync(adminUser, adminConfig.Password);
                if (createAdmin.Succeeded)
                {
                    Console.WriteLine("✅ Utente Admin creato correttamente.");
                    var assignRole = await userManager.AddToRoleAsync(adminUser, adminConfig.Role);
                    Console.WriteLine(assignRole.Succeeded
                        ? $"✅ Ruolo '{adminConfig.Role}' assegnato correttamente."
                        : $"❌ Errore nell'assegnazione del ruolo: {string.Join(", ", assignRole.Errors.Select(e => e.Description))}");
                }
                else
                {
                    Console.WriteLine(
                        $"❌ Errore nella creazione dell'utente Admin: {string.Join(", ", createAdmin.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ L'utente Admin esiste già.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Errore durante il seeding del database: {ex.Message}");
        }
    }

    /// <summary>
    ///     Verifica se un ruolo esiste già.
    /// </summary>
    private static async Task<bool> RoleExists(RoleManager<IdentityRole> roleManager, string roleName)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }

    /// <summary>
    ///     Seeding del db delle impostazioni
    /// </summary>
    /// <param name="serviceProvider"></param>
    private static async Task SeedSettingsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var settingsContext = scope.ServiceProvider.GetRequiredService<SettingsDbContext>();
        var secureStorage = scope.ServiceProvider.GetRequiredService<SecureStorage>();

        if (!settingsContext.Configurations.Any())
        {
            var defaultSmtpConfig = new SmtpConfig(secureStorage)
            {
                Host = "smtp.example.com",
                Port = 587,
                UseSsl = true,
                RequiresAuthentication = true,
                // Crittografiamo solo se SecureStorage è valido e i dati non sono già crittografati
                Username = SecureStorage.IsEncrypted("admin@healthgear.local")
                    ? "admin@healthgear.local"
                    : secureStorage.EncryptUsername("admin@healthgear.local"),
                Password = SecureStorage.IsEncrypted("Example123!")
                    ? "Example123!"
                    : secureStorage.EncryptPassword("Example123!")
            };

            var defaultConfig = new AppConfig
            {
                Smtp = defaultSmtpConfig,
                Logging = new LoggingConfig { LogLevel = LogLevelEnum.Information }
            };

            settingsContext.Configurations.Add(defaultConfig);
            await settingsContext.SaveChangesAsync();
        }
    }
}