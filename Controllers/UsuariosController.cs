using Microsoft.AspNetCore.Mvc;
using SGTA.Application.Features.Usuarios;
using SGTA.Application.Features.Usuarios.DTOs;
using sgta_backend.Common.Responses;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    // GET /api/usuarios/cuenta/{cuenta}
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
                Message = "No se encontró un usuario para la cuenta indicada.",
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
}
