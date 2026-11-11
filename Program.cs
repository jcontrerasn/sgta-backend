using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SGTA.Application.Exceptions;
using SGTA.Infrastructure.Persistence;
using SGTA.Application.Features.Roles;
using SGTA.Application.Features.Tareas;
using SGTA.Application.Features.UsuariosRoles;
using SGTA.Application.Features.Usuarios;
using SGTA.Application.Interfaces;
using SGTA.Infrastructure.Repositories;
using sgta_backend.Common.Responses;
using System.Reflection;

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
builder.Services.AddScoped<ITareaRepository, TareaRepository>();
builder.Services.AddScoped<TareaService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<IUsuarioRolRepository, UsuarioRolRepository>();
builder.Services.AddScoped<UsuarioRolService>();

// Servicios del backend
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SGTA Backend API",
        Version = "v1",
        Description = "API del Sistema de Gestión de Tareas Administrativas."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

// OpenAPI solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SGTA Backend API v1");
        options.RoutePrefix = "swagger";
    });
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
