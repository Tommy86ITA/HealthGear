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

// Aggiunta del servizio di gestione dei file
builder.Services.AddScoped<FileService>();

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

app.UseRouting(); // Abilita il sistema di routing

// Registrazione delle rotte con approccio consigliato (evita ASP0014)
app.MapControllerRoute(
    "device",
    "Device/{action=Index}/{id?}",
    new { controller = "Device" }
);

// Route per la home page (HomeController)
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}"
);

// Verifica che il database esista e crealo se necessario
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // Crea il database se non esiste
}


/*using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var inventoryNumberService = scope.ServiceProvider.GetRequiredService<InventoryNumberService>();

    // Recuperiamo tutti i dispositivi che non hanno ancora un numero di inventario
    var devicesToUpdate = await dbContext.Devices
        .Where(d => string.IsNullOrEmpty(d.InventoryNumber))
        .ToListAsync();

    // Per ogni dispositivo senza numero di inventario, generiamo e aggiorniamo il numero
    foreach (var device in devicesToUpdate)
    {
        var inventoryNumber = await inventoryNumberService.GenerateInventoryNumberAsync(device.DataCollaudo.Year);
        device.SetInventoryNumber(inventoryNumber); // Impostiamo il numero di inventario
    }

    // Salviamo i cambiamenti nel database
    await dbContext.SaveChangesAsync();

    Console.WriteLine($"Generati numeri di inventario per {devicesToUpdate.Count} dispositivi.");
}*/

app.Run();