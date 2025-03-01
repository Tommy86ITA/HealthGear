using System.Globalization;
using HealthGear.Data;
using HealthGear.Models;
using HealthGear.Services;
using HealthGear.Services.Reports;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF;
using QuestPDF.Infrastructure;
using SQLitePCL;

// Inizializzazione necessaria per SQLite
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

//
// 1. Configurazione del DbContext e di ASP.NET Core Identity
//

// Configura il DbContext per utilizzare SQLite, leggendo la connection string da appsettings.json.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura Identity per usare la classe ApplicationUser e IdentityRole.
// Qui usiamo AddIdentity per supportare i ruoli; rimuoviamo quindi AddDefaultIdentity per evitare duplicazioni.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Configurazione delle opzioni per la password
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        // Richiede email unica per ogni utente
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Registrazione dello stub per l'invio email (FakeEmailSender)
builder.Services.AddSingleton<IEmailSender, FakeEmailSender>();

//
// 2. Registrazione degli altri servizi dell'applicazione
//

// Aggiunge il supporto per controllers con views.
builder.Services.AddControllersWithViews();

// Aggiunge il supporto per le Razor Pages (necessario per le pagine di Identity).
builder.Services.AddRazorPages();

// Registra i servizi personalizzati.
builder.Services.AddScoped<DeadlineService>();
builder.Services.AddScoped<InventoryNumberService>();
builder.Services.AddScoped<PdfReportGenerator>();
builder.Services.AddScoped<ExcelReportGenerator>();

// Configura la licenza di QuestPDF (Community License).
Settings.License = LicenseType.Community;

// Configura il logging: rimuove provider esistenti e aggiunge il console logger.
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

//
// 3. Costruzione dell'applicazione
//

var app = builder.Build();

//
// 4. Configurazione della cultura
//

// Imposta la cultura italiana per date e numeri.
var cultureInfo = new CultureInfo("it-IT");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

//
// 5. Configurazione degli errori e della sicurezza
//

if (app.Environment.IsDevelopment())
{
    // Usa la Developer Exception Page in ambiente di sviluppo.
    app.UseDeveloperExceptionPage();
}
else
{
    // In produzione, usa un gestore degli errori e HSTS.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Abilita prima l'autenticazione e poi l'autorizzazione.
app.UseAuthentication();
app.UseAuthorization();

//
// 6. Configurazione del routing
//

// Route di default: se non specificato, il controller predefinito è Home e l'azione predefinita è Home.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}"
);

// Redirect esplicito della root ("/") alla pagina Home.
app.MapGet("/", context =>
{
    context.Response.Redirect("/Home");
    return Task.CompletedTask;
});

// Route per il DeviceController: tutte le richieste che iniziano con "Device" vengono gestite da DeviceController.
app.MapControllerRoute(
    name: "device",
    pattern: "Device/{action=Index}/{id?}",
    defaults: new { controller = "Device" }
);

// Mappa le Razor Pages (necessario per le pagine di Identity e altre Razor Pages).
app.MapRazorPages();

//
// 7. Verifica e creazione del database, e seeding dei ruoli
//

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Applica le migrazioni e crea tutte le tabelle necessarie (comprese quelle di Identity)
    dbContext.Database.Migrate();

    // Seeding dei ruoli: crea i ruoli "Admin" e "User" se non esistono
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = ["Admin", "User"];
    foreach (var roleName in roleNames)
    {
        if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
        }
    }
}

//
// 8. Avvio dell'applicazione
//

app.Run();