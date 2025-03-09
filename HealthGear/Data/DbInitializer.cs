using HealthGear.Constants;
using HealthGear.Models;
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
                if (result.Succeeded)
                    Console.WriteLine($"✅ Ruolo creato: {roleName}");
                else
                    Console.WriteLine(
                        $"❌ Errore nella creazione del ruolo {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var adminConfig = new AdminConfig();
            configuration.GetSection("DefaultAdmin").Bind(adminConfig);

            // Controlla che tutti i valori richiesti siano presenti nei User Secrets
            if (string.IsNullOrWhiteSpace(adminConfig.UserName) ||
                string.IsNullOrWhiteSpace(adminConfig.Email) ||
                string.IsNullOrWhiteSpace(adminConfig.Password) ||
                string.IsNullOrWhiteSpace(adminConfig.Role) ||
                string.IsNullOrWhiteSpace(adminConfig.FullName))
            {
                Console.WriteLine("⚠️ Configurazione Admin incompleta nei User Secrets. Verifica i valori.");
                return;
            }

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
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(adminUser, adminConfig.Password);
                if (createAdmin.Succeeded)
                {
                    Console.WriteLine("✅ Utente Admin creato correttamente.");
                    var assignRole = await userManager.AddToRoleAsync(adminUser, adminConfig.Role);
                    if (assignRole.Succeeded)
                        Console.WriteLine($"✅ Ruolo '{adminConfig.Role}' assegnato correttamente.");
                    else
                        Console.WriteLine(
                            $"❌ Errore nell'assegnazione del ruolo: {string.Join(", ", assignRole.Errors.Select(e => e.Description))}");
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
}