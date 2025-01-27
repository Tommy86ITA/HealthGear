using System.Globalization;
using HealthGear.Data;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

// Inizializzazione necessaria per evitare errori di runtime con SQLite
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

// Configurazione del database SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Aggiunta dei servizi MVC con supporto per le viste Razor
builder.Services.AddControllersWithViews();

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

app.UseHttpsRedirection();  // Forza l'uso di HTTPS
app.UseStaticFiles();       // Abilita la gestione dei file statici (CSS, JS, immagini)

app.UseRouting();  // Abilita il sistema di routing

// Registrazione delle rotte con approccio consigliato (evita ASP0014)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Verifica che il database esista e crealo se necessario
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();  // Crea il database se non esiste
}

app.Run();