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

// 1. Configurazione del database e Identity per la gestione degli utenti
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), opt =>
        opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

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

// 3. Configurazione dei servizi MVC, Razor Pages e gestione della sessione
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddRazorPages();

Settings.License = LicenseType.Community;

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

// 14. Avvio dell'applicazione
app.Run();