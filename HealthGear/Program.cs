using System.Globalization;
using HealthGear.Data;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

Batteries.Init(); // Necessario per evitare l'errore di runtime

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurazione della cultura italiana per il formato data
var cultureInfo = new CultureInfo("it-IT");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();
app.Run();