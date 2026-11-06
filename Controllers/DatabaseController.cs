
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGTA.Infrastructure.Persistence;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly SgtaDbContext _context;

    public DatabaseController(SgtaDbContext context)
    {
        _context = context;
    }

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
