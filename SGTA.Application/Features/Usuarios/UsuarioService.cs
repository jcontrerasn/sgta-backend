using SGTA.Application.Features.Usuarios.DTOs;
using SGTA.Application.Interfaces;

namespace SGTA.Application.Features.Usuarios;

public class UsuarioService
{
    private readonly IUsuarioExternoRepository _repository;

    public UsuarioService(IUsuarioExternoRepository repository)
    {
        _repository = repository;
    }

    public async Task<UsuarioCuentaResponse?> BuscarPorCuentaAsync(
        string cuenta,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cuenta))
        {
            throw new ArgumentException(
                "La cuenta es obligatoria.", nameof(cuenta));
        }

        var usuario = await _repository.BuscarPorCuentaAsync(
            cuenta.Trim(), cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        return new UsuarioCuentaResponse
        {
            Usuario = usuario.Usuario,
            NombreCompleto = usuario.NombreCompleto,
            Area = usuario.Area
        };
    }
}
