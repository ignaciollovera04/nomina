
using Aplicacion;
using Aplicacion.Servicios;
using Microsoft.Data.SqlClient;
using Persistencia.Interfaces;
using Persistencia.Repositorios;
using System.Data;

using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

var builder = WebApplication.CreateBuilder(args);

// Leer la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("cadenaSQL");

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddScoped<IDbConnection>((sp) =>
    new SqlConnection(connectionString)
);

QuestPDF.Settings.License = LicenseType.Community;

// Configuración de Dapper y Repositorios
builder.Services.AddScoped<IContratoRepository, ContratoRepository>();
builder.Services.AddScoped<IPeriodoNominaRepository, PeriodoNominaRepository>();
builder.Services.AddScoped<INominaRepository, NominaRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<ICargoRepository, CargoRepository>();


// Servicios de Aplicación
builder.Services.AddScoped<INominaService, NominaService>();
builder.Services.AddScoped<IContratoService, ContratoService>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Nomina}/{action=Index}/{id?}");

app.Run();
