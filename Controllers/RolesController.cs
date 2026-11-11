
using Microsoft.AspNetCore.Mvc;
using SGTA.Application.Features.Roles;
using SGTA.Application.Features.Roles.DTOs;
using sgta_backend.Common.Responses;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/roles")]
/// <summary>
/// Operaciones para consultar y crear roles.
/// </summary>
public class RolesController : ControllerBase
{
    private readonly RolService _rolService;

    public RolesController(RolService rolService)
    {
        _rolService = rolService;
    }

    // GET /api/roles
    /// <summary>
    /// Obtiene todos los roles registrados.
    /// </summary>
    /// <response code="200">Lista de roles obtenida correctamente.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<List<RolResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> ObtenerRoles(
        CancellationToken cancellationToken)
    {
        var roles = await _rolService.ObtenerTodosAsync(
            cancellationToken);

        return Ok(new ApiResponse<List<RolResponse>>
        {
            Status = 200,
            Message = "Roles obtenidos correctamente",
            Data = roles
        });
    }

    // POST /api/roles
    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    /// <param name="request">Datos del rol que se desea crear.</param>
    /// <response code="201">Rol creado correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="409">Ya existe un rol con el mismo nombre.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<RolResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> CrearRol(
        [FromBody] CrearRolRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = StatusCodes.Status400BadRequest,
                Message = "El nombre del rol es obligatorio",
                Data = null
            });
        }

        var rolCreado = await _rolService.CrearAsync(
            request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<RolResponse>
        {
            Status = StatusCodes.Status201Created,
            Message = "Rol creado correctamente",
            Data = rolCreado
        });
    }
}
