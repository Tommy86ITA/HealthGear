using System.Globalization;
using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Models.Settings;
using HealthGear.Services;
using HealthGear.Services.Reports;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF;
using QuestPDF.Infrastructure;
using SQLitePCL;

// Inizializzazione necessaria per SQLite su Linux/macOS
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

//
// 1. Configurazione DbContext e Identity
//
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var passwordRules = new PasswordRules();
builder.Configuration.GetSection("PasswordRules").Bind(passwordRules);
builder.Services.AddSingleton(passwordRules);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = passwordRules.RequireDigit;
        options.Password.RequireLowercase = passwordRules.RequireLowercase;
        options.Password.RequireUppercase = passwordRules.RequireUppercase;
        options.Password.RequireNonAlphanumeric = passwordRules.RequireNonAlphanumeric;
        options.Password.RequiredLength = passwordRules.MinLength;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<ItalianIdentityErrorDescriber>();

//
// 2. Registrazione dei servizi applicativi custom
//
builder.Services.AddSingleton<PasswordGenerator>();
builder.Services.AddScoped<PasswordValidator>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailSender>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<DeadlineService>();
builder.Services.AddScoped<InventoryNumberService>();
builder.Services.AddScoped<PdfReportGenerator>();
builder.Services.AddScoped<ExcelReportGenerator>();
builder.Services.AddSingleton<TemporaryPasswordCacheService>();
builder.Services.AddSingleton<ThirdPartyService>();

//
// 3. Configurazione servizi MVC, Razor Pages e Session/TempData
//
builder.Services.AddSession();
builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();
builder.Services.AddRazorPages();

Settings.License = LicenseType.Community;

//
// 4. Configurazione logging (utile per debug su console di Rider o terminale)
//
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

//
// 5. Costruzione applicazione
//
var app = builder.Build();

//
// 6. Configurazione Cultura (italiana)
//
var cultureInfo = new CultureInfo("it-IT");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

//
// 7. Middleware per ambienti di sviluppo/produzione
//
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

//
// 8. Log globale di debug per intercettare TUTTE le richieste
//
/*app.Use(async (context, next) =>
{
    Console.WriteLine($"[DEBUG] Richiesta: {context.Request.Method} {context.Request.Path}");
    await next();
});*/

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

//
// 9. Configurazione rotte principali
//
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Home}/{id?}");

app.MapRazorPages();

//
// 10. Redirect di default (se l'utente è autenticato, va a /Home/Home, altrimenti al login)
//
app.MapGet("/", async context =>
{
    if (context.User.Identity?.IsAuthenticated ?? false)
        context.Response.Redirect("/Home/Home");
    else
        context.Response.Redirect("/Identity/Account/Login");

    await Task.CompletedTask;
});

//
// 11. Inizializzazione database e seeding
//
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🔄 Verifica e creazione database se necessario...");
        dbContext.Database.EnsureCreated(); // Garantisce che il database esista

        logger.LogInformation("🔄 Avvio della migrazione del database...");
        dbContext.Database.Migrate(); // Applica le migrazioni
        logger.LogInformation("✅ Migrazione completata con successo!");

        logger.LogInformation("🏁 Avvio del seeding dei dati...");
        await DbInitializer.SeedDataAsync(services, builder.Configuration);
        logger.LogInformation("✅ Seeding completato con successo!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Errore durante l'inizializzazione del database.");
        throw;
    }
}

//
// 12. Avvio applicazione
//

app.Run();