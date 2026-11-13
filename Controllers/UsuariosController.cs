using Microsoft.AspNetCore.Mvc;
using SGTA.Application.Features.Usuarios;
using SGTA.Application.Features.Usuarios.DTOs;
using sgta_backend.Common.Responses;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/usuarios")]
/// <summary>
/// Consultas de usuarios de BDSERVER01.
/// </summary>
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    // GET /api/usuarios/cuenta/{cuenta}
    /// <summary>
    /// Busca un usuario por su número de cuenta y valida que tenga un rol activo.
    /// </summary>
    /// <param name="cuenta">Número de cuenta que se desea consultar.</param>
    /// <response code="200">Usuario encontrado y registrado con un rol activo.</response>
    /// <response code="404">El usuario no está registrado o no tiene un rol activo.</response>
    /// <response code="400">La cuenta no fue informada correctamente.</response>
    /// <response code="500">Ocurrió un error al consultar BDSERVER01.</response>
    [ProducesResponseType(typeof(ApiResponse<UsuarioCuentaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UsuarioCuentaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet("cuenta/{cuenta}")]
    public async Task<IActionResult> BuscarPorCuenta(
        string cuenta,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.BuscarPorCuentaAsync(
            cuenta, cancellationToken);

        if (usuario is null)
        {
            return NotFound(new ApiResponse<UsuarioCuentaResponse>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "Este usuario no está registrado.",
                Data = null
            });
        }

        return Ok(new ApiResponse<UsuarioCuentaResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Usuario obtenido correctamente.",
            Data = usuario
        });
    }

    /// <summary>
    /// Lista usuarios usando el procedimiento almacenado de búsqueda por filtro.
    /// </summary>
    /// <param name="request">Filtro enviado al procedimiento almacenado.</param>
    /// <response code="200">Lista de usuarios obtenida correctamente.</response>
    /// <response code="400">El filtro no fue informado correctamente.</response>
    /// <response code="500">Ocurrió un error al consultar BDSERVER01.</response>
    [ProducesResponseType(typeof(ApiResponse<List<UsuarioCuentaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPost("buscar-por-filtro")]
    public async Task<IActionResult> BuscarPorFiltro(
        [FromBody] BuscarUsuariosPorFiltroRequest request,
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioService.BuscarPorFiltroAsync(
            request.Pfiltro_c, cancellationToken);

        return Ok(new ApiResponse<List<UsuarioCuentaResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = usuarios.Count == 0
                ? "No se encontraron usuarios para el filtro indicado."
                : "Usuarios obtenidos correctamente.",
            Data = usuarios
        });
    }

    /// <summary>
    /// Lista personal de TrackIT usando el procedimiento almacenado de bÃºsqueda por filtro.
    /// </summary>
    /// <param name="request">Filtro enviado al procedimiento almacenado.</param>
    /// <response code="200">Personal de TrackIT obtenido correctamente.</response>
    /// <response code="400">El filtro no fue informado correctamente.</response>
    /// <response code="500">OcurriÃ³ un error al consultar BDSERVER01.</response>
    [ProducesResponseType(typeof(ApiResponse<List<PersonalTrackItResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPost("personal-trackit/buscar-por-filtro")]
    public async Task<IActionResult> BuscarPersonalTrackItPorFiltro(
        [FromBody] BuscarUsuariosPorFiltroRequest request,
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioService.BuscarPersonalTrackItPorFiltroAsync(
            request.Pfiltro_c, cancellationToken);

        return Ok(new ApiResponse<List<PersonalTrackItResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = usuarios.Count == 0
                ? "No se encontrÃ³ personal de TrackIT para el filtro indicado."
                : "Personal de TrackIT obtenido correctamente.",
            Data = usuarios
        });
    }
}
