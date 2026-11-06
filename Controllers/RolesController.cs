
using Microsoft.AspNetCore.Mvc;
using SGTA.Application.Features.Roles;
using SGTA.Application.Features.Roles.DTOs;
using sgta_backend.Common.Responses;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly RolService _rolService;

    public RolesController(RolService rolService)
    {
        _rolService = rolService;
    }

    // GET /api/roles
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
