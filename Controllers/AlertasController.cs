using Microsoft.AspNetCore.Mvc;
using SGTA.Application.Features.Alertas;
using SGTA.Application.Features.Alertas.DTOs;
using sgta_backend.Common.Responses;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/alertas")]
public class AlertasController : ControllerBase
{
    private readonly AlertaService _service;

    public AlertasController(AlertaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene el historial de alertas y sus destinatarios.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Historial de alertas obtenido correctamente.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(
        typeof(ApiResponse<List<AlertaResponse>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status500InternalServerError)]
    [HttpGet("historial")]
    public async Task<IActionResult> ObtenerHistorial(
        CancellationToken cancellationToken)
    {
        var alertas = await _service.ObtenerHistorialAsync(
            cancellationToken);

        return Ok(new ApiResponse<List<AlertaResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = alertas.Count == 0
                ? "No se encontraron alertas registradas."
                : "Historial de alertas obtenido correctamente.",
            Data = alertas
        });
    }
}
