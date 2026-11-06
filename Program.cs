using Microsoft.EntityFrameworkCore;
using SGTA.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SgtaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SgtaConnection")
    )
);

// Servicios del backend
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Archivos estáticos de React
// app.UseDefaultFiles();
// app.UseStaticFiles();

// Endpoints de la API
app.MapControllers();

// Rutas del frontend React
// app.MapFallbackToFile("index.html");

app.Run();
