using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Models.Config;
using HealthGear.Models.Settings;
using HealthGear.Services;
using HealthGear.Services.Reports;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
#if !DESIGN_TIME
using QuestPDF;
using QuestPDF.Infrastructure;
#endif
using SQLitePCL;
using System.Globalization;
using System.Text.Json;

// Inizializzazione necessaria per SQLite su Linux/macOS
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);
var logger = LoggerFactory
    .Create(b => b.AddConsole())
    .CreateLogger("Startup");

// 1. Configurazione del database e Identity per la gestione degli utenti
var configPath = Path.Combine(@"C:\ProgramData\HealthGear Suite\HealthGearConfig", "healthgear.config.json");
HealthGearConfig hgConfig = HealthGearConfig.CreateDefault();

if (File.Exists(configPath))
{
    try
    {
        var json = File.ReadAllText(configPath);
        hgConfig = JsonSerializer.Deserialize<HealthGearConfig>(json) ?? HealthGearConfig.CreateDefault();
        logger.LogInformation("✅ Configurazione caricata da healthgear.config.json");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "⚠️ Errore durante la lettura del file di configurazione. Verranno usati i valori di default.");
        hgConfig = HealthGearConfig.CreateDefault();
    }
}
else
{
    logger.LogWarning("⚠️ File di configurazione healthgear.config.json non trovato. Verranno usati i valori di default.");
}

ApplyConnectionStrings();

var settingsDbPath = $"Data Source={hgConfig.SettingsDbPath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), opt =>
        opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddDbContext<SettingsDbContext>(options =>
    options.UseSqlite(settingsDbPath));

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

// 2. Registrazione dei servizi applicativi custom
builder.Services.AddScoped<SettingsService>();
builder.Services.AddSingleton<PasswordGenerator>();
builder.Services.AddScoped<PasswordValidator>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailSender>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<DeadlineService>();
builder.Services.AddScoped<InventoryNumberService>();
builder.Services.AddScoped<QrCodeService>();
builder.Services.AddScoped<PdfReportGenerator>();
builder.Services.AddScoped<ExcelReportGenerator>();
builder.Services.AddSingleton<TemporaryPasswordCacheService>();
builder.Services.AddSingleton<ThirdPartyService>();

// 🔐 Abilita Data Protection per la crittografia sicura delle credenziali SMTP
builder.Services.AddDataProtection();
builder.Services.AddSingleton<SecureStorage>();

// 3. Configurazione dei servizi MVC, Razor Pages e gestione della sessione
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddRazorPages();

#if !DESIGN_TIME
Settings.License = LicenseType.Community;
#endif

// 4. Configurazione del logging per la console di debug
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// 10. Configurazione dei percorsi di autenticazione e accesso negato
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/StatusPages/AccessDenied";
});

// 5. Creazione dell'applicazione
builder.Services.AddSingleton(hgConfig);

var app = builder.Build();

// 6. Impostazione della cultura italiana come predefinita
var cultureInfo = new CultureInfo("it-IT");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// 7. Configurazione middleware per ambienti di sviluppo e produzione
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 8. Configurazione globale dei middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// 9. Gestione delle pagine di errore (403 Accesso Negato)
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    switch (response.StatusCode)
    {
        case 403:
            response.Redirect("/StatusPages/AccessDenied");
            break;
        case 404:
            response.Redirect("/StatusPages/NotFound");
            break;
        default:
            response.Redirect("/StatusPages/Error");
            break;
    }

    await Task.CompletedTask;
});

// 11. Configurazione delle rotte principali
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Home}/{id?}");
app.MapRazorPages();

// 12. Redirect automatico all'accesso
app.MapGet("/", async context =>
{
    if (context.User.Identity?.IsAuthenticated ?? false)
        context.Response.Redirect("/Home/Home");
    else
        context.Response.Redirect("/Identity/Account/Login");

    await Task.CompletedTask;
});

// 13. Inizializzazione del database e seeding dei dati
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    var settingsDbContext = services.GetRequiredService<SettingsDbContext>();
    var scopedLogger = services.GetRequiredService<ILogger<Program>>();
    var mainDbPath = hgConfig.DatabasePath;
    scopedLogger.LogInformation($"🗄️ Il database usato per ApplicationDbContext è: {mainDbPath}");
    var dbPath = hgConfig.SettingsDbPath;
    scopedLogger.LogInformation($"🗄️ Il database usato per SettingsDbContext è: {dbPath}");

    try
    {
        scopedLogger.LogInformation("🔄 Applicazione delle migrazioni al database principale...");
        dbContext.Database.Migrate();
        scopedLogger.LogInformation("✅ Migrazione database principale completata!");

        scopedLogger.LogInformation("🔄 Applicazione delle migrazioni al database impostazioni...");
        settingsDbContext.Database.Migrate();
        scopedLogger.LogInformation("✅ Migrazione database impostazioni completata!");

        scopedLogger.LogInformation("🏁 Avvio del seeding dei dati...");
        await DbInitializer.SeedDataAsync(services, builder.Configuration);
        scopedLogger.LogInformation("✅ Seeding completato con successo!");

        // Ottenere il servizio delle impostazioni
        var settingsService = services.GetRequiredService<SettingsService>();

        // Verifica e carica le impostazioni dal database
        var config = await settingsService.GetConfigAsync();
        if (config != null)
        {
            scopedLogger.LogInformation("✅ Configurazione caricata con successo.");
        }
        else
        {
            scopedLogger.LogWarning(
                "⚠️ Nessuna configurazione trovata nel database. Verranno utilizzati i valori di default.");
        }
    }
    catch (Exception ex)
    {
        scopedLogger.LogError(ex, "❌ Errore durante l'inizializzazione dei database.");
        scopedLogger.LogWarning(
            "⚠️ L'applicazione continuerà l'esecuzione, ma alcune funzionalità potrebbero non essere disponibili.");
    }
}

// 14. Avvio dell'applicazione
app.Run();

void ApplyConnectionStrings()
{
    // 🛠️ Verifica e crea le directory dei file .db, se non esistono
    var dbDir = Path.GetDirectoryName(hgConfig.DatabasePath);
    if (!string.IsNullOrWhiteSpace(dbDir) && !Directory.Exists(dbDir))
        Directory.CreateDirectory(dbDir);

    var settingsDir = Path.GetDirectoryName(hgConfig.SettingsDbPath);
    if (!string.IsNullOrWhiteSpace(settingsDir) && !Directory.Exists(settingsDir))
        Directory.CreateDirectory(settingsDir);

    // 🧱 Applica valori di fallback se mancano
    if (string.IsNullOrWhiteSpace(hgConfig.DatabasePath))
        hgConfig.DatabasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "healthgear.db");

    if (string.IsNullOrWhiteSpace(hgConfig.SettingsDbPath))
        hgConfig.SettingsDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.db");

    builder.Configuration["ConnectionStrings:DefaultConnection"] = $"Data Source={hgConfig.DatabasePath}";
    builder.Configuration["ConnectionStrings:SettingsConnection"] = $"Data Source={hgConfig.SettingsDbPath}";
}