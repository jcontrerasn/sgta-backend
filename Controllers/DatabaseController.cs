
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGTA.Infrastructure.Persistence;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/database")]
/// <summary>
/// Endpoints para verificar la conectividad con la base de datos principal.
/// </summary>
public class DatabaseController : ControllerBase
{
    private readonly SgtaDbContext _context;

    public DatabaseController(SgtaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Abre y cierra una conexión contra SQL Server para comprobar su disponibilidad.
    /// </summary>
    /// <response code="200">La conexión se estableció correctamente.</response>
    /// <response code="503">SQL Server no está disponible.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        try
        {
            await _context.Database.OpenConnectionAsync();

            await _context.Database.CloseConnectionAsync();

            return Ok(new
            {
                mensaje = "Conexión exitosa a SQL Server",
                conectado = true
            });
        }
        catch (Exception ex)
        {
            // Solo para diagnóstico local
            Console.WriteLine(ex.ToString());

            return StatusCode(503, new
            {
                mensaje = "Error al conectar a SQL Server",
                detalle = ex.Message
            });
        }
    }
}
