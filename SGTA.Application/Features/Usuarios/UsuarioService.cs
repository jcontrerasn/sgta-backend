using SGTA.Application.Features.Usuarios.DTOs;
using SGTA.Application.Interfaces;

namespace SGTA.Application.Features.Usuarios;

public class UsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IUsuarioRolRepository _usuarioRolRepository;

    public UsuarioService(
        IUsuarioRepository repository,
        IUsuarioRolRepository usuarioRolRepository)
    {
        _repository = repository;
        _usuarioRolRepository = usuarioRolRepository;
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

        var cuentaValidada = cuenta.Trim();

        var rol = await _usuarioRolRepository.ObtenerNombreRolActivoAsync(
            cuentaValidada, cancellationToken);

        if (string.IsNullOrWhiteSpace(rol))
        {
            return null;
        }

        var usuario = await _repository.BuscarPorCuentaAsync(
            cuentaValidada, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(usuario.UsuarioNombre))
        {
            return null;
        }

        return new UsuarioCuentaResponse
        {
            Usuario = usuario.UsuarioNombre,
            NombreCompleto = usuario.NombreCompleto,
            Area = usuario.Area,
            Rol = rol
        };
    }

    public async Task<List<UsuarioCuentaResponse>> BuscarPorFiltroAsync(
        string filtro,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filtro))
        {
            throw new ArgumentException(
                "El filtro es obligatorio.", nameof(filtro));
        }

        var usuarios = await _repository.BuscarPorFiltroAsync(
            filtro.Trim(), cancellationToken);

        return usuarios.Select(usuario => new UsuarioCuentaResponse
        {
            Usuario = usuario.UsuarioNombre,
            NombreCompleto = usuario.NombreCompleto,
            Area = usuario.Area
        }).ToList();
    }

    public async Task<List<PersonalTrackItResponse>> BuscarPersonalTrackItPorFiltroAsync(
        string filtro,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filtro))
        {
            throw new ArgumentException(
                "El filtro es obligatorio.", nameof(filtro));
        }

        var usuarios = await _repository.BuscarPersonalTrackItPorFiltroAsync(
            filtro.Trim(), cancellationToken);

        return usuarios.Select(usuario => new PersonalTrackItResponse
        {
            Usuario = usuario.UsuarioNombre,
            NombreCompleto = usuario.NombreCompleto,
            Area = usuario.Area
        }).ToList();
    }
}
