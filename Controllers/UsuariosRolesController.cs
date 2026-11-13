using Microsoft.AspNetCore.Mvc;
using SGTA.Application.Features.UsuariosRoles;
using SGTA.Application.Features.UsuariosRoles.DTOs;
using sgta_backend.Common.Responses;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/usuario-roles")]
/// <summary>
/// Consultas de asignaciones de roles por usuario.
/// </summary>
public class UsuariosRolesController : ControllerBase
{
    private readonly UsuarioRolService _usuarioRolService;

    public UsuariosRolesController(UsuarioRolService usuarioRolService)
    {
        _usuarioRolService = usuarioRolService;
    }

    // POST /api/usuario-roles
    /// <summary>
    /// Asigna un rol a un usuario.
    /// </summary>
    /// <param name="request">Datos de la asignación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="201">Asignación creada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="409">La asignación ya existe.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<UsuarioRolResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUsuarioRolRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioRol = await _usuarioRolService.CrearAsync(
            request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<UsuarioRolResponse>
            {
                Status = StatusCodes.Status201Created,
                Message = "Asignación usuario-rol creada correctamente.",
                Data = usuarioRol
            });
    }

    // PUT /api/usuario-roles/{idUsuarioRol}
    /// <summary>
    /// Actualiza el usuario o rol de una asignación.
    /// </summary>
    /// <param name="idUsuarioRol">Identificador de la asignación.</param>
    /// <param name="request">Datos actualizados de la asignación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Asignación actualizada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No se encontró la asignación indicada.</response>
    /// <response code="409">La asignación ya existe.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<UsuarioRolResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPut("{idUsuarioRol:int}")]
    public async Task<IActionResult> Actualizar(
        [FromRoute] int idUsuarioRol,
        [FromBody] ActualizarUsuarioRolRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioRol = await _usuarioRolService.ActualizarAsync(
            idUsuarioRol, request, cancellationToken);

        if (usuarioRol is null)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontró la asignación indicada.",
                Data = null
            });
        }

        return Ok(new ApiResponse<UsuarioRolResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Asignación usuario-rol actualizada correctamente.",
            Data = usuarioRol
        });
    }

    // PATCH /api/usuario-roles/{idUsuarioRol}/estado
    /// <summary>
    /// Invierte el estado activo de una asignación usuario-rol.
    /// </summary>
    /// <param name="idUsuarioRol">Identificador de la asignación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Estado actualizado correctamente.</response>
    /// <response code="400">El identificador no es válido.</response>
    /// <response code="404">No se encontró la asignación indicada.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<UsuarioRolResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPatch("{idUsuarioRol:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        [FromRoute] int idUsuarioRol,
        CancellationToken cancellationToken)
    {
        var usuarioRol = await _usuarioRolService.CambiarEstadoAsync(
            idUsuarioRol, cancellationToken);

        if (usuarioRol is null)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontró la asignación indicada.",
                Data = null
            });
        }

        return Ok(new ApiResponse<UsuarioRolResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Estado de la asignación actualizado correctamente.",
            Data = usuarioRol
        });
    }

    // DELETE /api/usuario-roles/{idUsuarioRol}
    /// <summary>
    /// Elimina una asignaciÃ³n usuario-rol por su identificador.
    /// </summary>
    /// <param name="idUsuarioRol">Identificador de la asignaciÃ³n usuario-rol.</param>
    /// <param name="cancellationToken">Token para cancelar la operaciÃ³n.</param>
    /// <response code="200">AsignaciÃ³n eliminada correctamente.</response>
    /// <response code="400">El identificador no es vÃ¡lido.</response>
    /// <response code="404">No se encontrÃ³ la asignaciÃ³n indicada.</response>
    /// <response code="500">OcurriÃ³ un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpDelete("{idUsuarioRol:int}")]
    public async Task<IActionResult> Eliminar(
        [FromRoute] int idUsuarioRol,
        CancellationToken cancellationToken)
    {
        var eliminado = await _usuarioRolService.EliminarAsync(
            idUsuarioRol, cancellationToken);

        if (!eliminado)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontrÃ³ la asignaciÃ³n indicada.",
                Data = null
            });
        }

        return Ok(new ApiResponse<object>
        {
            Status = StatusCodes.Status200OK,
            Message = "Usuario eliminado correctamente.",
            Data = null
        });
    }

    /// <summary>
    /// Obtiene todas las relaciones entre usuarios y roles.
    /// </summary>
    /// <response code="200">Relaciones obtenidas correctamente.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<List<UsuarioRolResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> ObtenerUsuariosRoles(
        CancellationToken cancellationToken)
    {
        var usuariosRoles = await _usuarioRolService.ObtenerTodosAsync(
            cancellationToken);

        return Ok(new ApiResponse<List<UsuarioRolResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Relaciones usuario-rol obtenidas correctamente.",
            Data = usuariosRoles
        });
    }
}
