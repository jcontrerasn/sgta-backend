using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SGTA.Application.Exceptions;
using SGTA.Infrastructure.Persistence;
using SGTA.Application.Features.Roles;
using SGTA.Application.Features.Usuarios;
using SGTA.Application.Interfaces;
using SGTA.Infrastructure.Repositories;
using sgta_backend.Common.Responses;

var builder = WebApplication.CreateBuilder(args);

var primaryConnectionString = builder.Configuration
    .GetConnectionString("SgtaConnection");

if (string.IsNullOrWhiteSpace(primaryConnectionString))
{
    throw new InvalidOperationException(
        "No se encontró ConnectionStrings:SgtaConnection en la configuración.");
}

builder.Services.AddDbContext<SgtaDbContext>(options =>
    options.UseSqlServer(
        primaryConnectionString
    )
);

builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<IUsuarioExternoRepository, UsuarioExternoRepository>();
builder.Services.AddScoped<UsuarioService>();

// Servicios del backend
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Respuestas de error con el formato estándar de la API
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?.Error;

        if (exception is ConflictException conflictException)
        {
            app.Logger.LogWarning(
                "Conflicto de negocio: {Message}",
                conflictException.Message);
        }
        else if (exception is ArgumentException argumentException)
        {
            app.Logger.LogWarning(
                "Solicitud inválida: {Message}",
                argumentException.Message);
        }
        else if (exception is not null)
        {
            app.Logger.LogError(
                exception,
                "Error no controlado en la API");
        }

        var (status, message) = exception switch
        {
            ConflictException conflict =>
                (StatusCodes.Status409Conflict, conflict.Message),
            ArgumentException argument =>
                (StatusCodes.Status400BadRequest, argument.Message),
            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Ocurrió un error interno en el servidor")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            new ApiResponse<object>
            {
                Status = status,
                Message = message,
                Data = null
            });
    });
});

// Archivos estáticos de React
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoints de la API
app.MapControllers();

// Rutas del frontend React
// app.MapFallbackToFile("index.html");

app.Run();
