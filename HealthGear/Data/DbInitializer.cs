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

        string[] roleNames = [Roles.Admin, Roles.Tecnico, Roles.Office];

        foreach (var roleName in roleNames)
        {
            if (await RoleExists(roleManager, roleName)) continue;
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Console.WriteLine($"✅ Ruolo creato: {roleName}");
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
            Console.WriteLine("⚠️ Configurazione Admin incompleta nei User Secrets.");
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
            }
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