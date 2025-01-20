using HealthGear.Data;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

Batteries.Init(); // Necessario per evitare l'errore di runtime

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();
app.Run();