#region

using System.Globalization;
using HealthGear.Data;
using HealthGear.Services;
using HealthGear.Services.Reports;
using Microsoft.EntityFrameworkCore;
using QuestPDF;
using QuestPDF.Infrastructure;
using SQLitePCL;

#endregion

// Inizializzazione necessaria per evitare errori di runtime con SQLite
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

// Configurazione del database SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Aggiunta dei servizi MVC con supporto per le viste Razor
builder.Services.AddControllersWithViews();

// Aggiunta del servizio di calcolo delle scadenze
builder.Services.AddScoped<DeadlineService>();

// Aggiunta del servizio di generazione del numero di inventario
builder.Services.AddScoped<InventoryNumberService>();

// Aggiunta dei servizi di generazione dei report
builder.Services.AddScoped<PdfReportGenerator>();
builder.Services.AddScoped<ExcelReportGenerator>();

// Configura la licenza di QuestPDF
Settings.License = LicenseType.Community;

// Configurazione del logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

// Configurazione della cultura italiana per il formato data
var cultureInfo = new CultureInfo("it-IT");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Gestione degli errori basata sull'ambiente
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Abilita HSTS per una maggiore sicurezza
}

app.UseHttpsRedirection(); // Forza l'uso di HTTPS
app.UseStaticFiles(); // Abilita la gestione dei file statici (CSS, JS, immagini)
app.UseRouting();

// Route di default: adesso il controller predefinito è Home e l'azione predefinita è "Home" (che restituisce la view "Home.cshtml")
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Home}/{id?}"
);

// Redirect esplicito della root ("/") a "/Home"
app.MapGet("/", context =>
{
    context.Response.Redirect("/Home");
    return Task.CompletedTask;
});

// Route per il DeviceController: tutte le richieste che iniziano con "Device" verranno gestite da DeviceController
app.MapControllerRoute(
    "device",
    "Device/{action=Index}/{id?}",
    new { controller = "Device" }
);

// Verifica che il database esista e crealo se necessario
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // Crea il database se non esiste
}

app.Run();