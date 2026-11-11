using SGTA.Application.Features.UsuariosRoles.DTOs;
using SGTA.Application.Exceptions;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;

namespace SGTA.Application.Features.UsuariosRoles;

public class UsuarioRolService
{
    private readonly IUsuarioRolRepository _repository;

    public UsuarioRolService(IUsuarioRolRepository repository)
    {
        _repository = repository;
    }

    public async Task<UsuarioRolResponse> CrearAsync(
        CrearUsuarioRolRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuario = ValidarUsuario(request.Usuario);
        ValidarIdRol(request.IdRol);

        var nombreRol = await ValidarRolAsync(
            request.IdRol, cancellationToken);
        await ValidarAsignacionDuplicadaAsync(
            usuario, request.IdRol, null, cancellationToken);

        var usuarioRol = await _repository.CrearAsync(
            new UsuarioRol
            {
                Usuario = usuario,
                IdRol = request.IdRol,
                NombreRol = nombreRol,
                Activo = true
            },
            cancellationToken);

        return MapearRespuesta(usuarioRol);
    }

    public async Task<UsuarioRolResponse?> ActualizarAsync(
        int idUsuarioRol,
        ActualizarUsuarioRolRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarIdUsuarioRol(idUsuarioRol);

        var usuario = ValidarUsuario(request.Usuario);
        ValidarIdRol(request.IdRol);

        var nombreRol = await ValidarRolAsync(
            request.IdRol, cancellationToken);
        await ValidarAsignacionDuplicadaAsync(
            usuario, request.IdRol, idUsuarioRol, cancellationToken);

        var usuarioRol = await _repository.ActualizarAsync(
            new UsuarioRol
            {
                IdUsuarioRol = idUsuarioRol,
                Usuario = usuario,
                IdRol = request.IdRol,
                NombreRol = nombreRol
            },
            cancellationToken);

        return usuarioRol is null
            ? null
            : MapearRespuesta(usuarioRol);
    }

    public async Task<UsuarioRolResponse?> CambiarEstadoAsync(
        int idUsuarioRol,
        CancellationToken cancellationToken = default)
    {
        ValidarIdUsuarioRol(idUsuarioRol);

        var usuarioRol = await _repository.CambiarEstadoAsync(
            idUsuarioRol, cancellationToken);

        return usuarioRol is null
            ? null
            : MapearRespuesta(usuarioRol);
    }

    public async Task<List<UsuarioRolResponse>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var usuariosRoles = await _repository.ObtenerTodosAsync(
            cancellationToken);

        return usuariosRoles.Select(usuarioRol => new UsuarioRolResponse
        {
            IdUsuarioRol = usuarioRol.IdUsuarioRol,
            Usuario = usuarioRol.Usuario,
            NombreRol = usuarioRol.NombreRol,
            Activo = usuarioRol.Activo
        }).ToList();
    }

    private async Task<string> ValidarRolAsync(
        int idRol,
        CancellationToken cancellationToken)
    {
        var nombreRol = await _repository.ObtenerNombreRolAsync(
            idRol, cancellationToken);

        if (string.IsNullOrWhiteSpace(nombreRol))
        {
            throw new ArgumentException(
                "No existe el rol indicado.");
        }

        return nombreRol;
    }

    private async Task ValidarAsignacionDuplicadaAsync(
        string usuario,
        int idRol,
        int? idUsuarioRolExcluir,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExisteAsignacionAsync(
            usuario, idRol, idUsuarioRolExcluir, cancellationToken))
        {
            throw new ConflictException(
                "Ya existe una asignación para ese usuario y rol.");
        }
    }

    private static string ValidarUsuario(string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
        {
            throw new ArgumentException(
                "El usuario es obligatorio.");
        }

        return usuario.Trim();
    }

    private static void ValidarIdRol(int idRol)
    {
        if (idRol <= 0)
        {
            throw new ArgumentException(
                "El Id del rol debe ser mayor que cero.");
        }
    }

    private static void ValidarIdUsuarioRol(int idUsuarioRol)
    {
        if (idUsuarioRol <= 0)
        {
            throw new ArgumentException(
                "El Id de la asignación debe ser mayor que cero.");
        }
    }

    private static UsuarioRolResponse MapearRespuesta(
        UsuarioRol usuarioRol)
    {
        return new UsuarioRolResponse
        {
            IdUsuarioRol = usuarioRol.IdUsuarioRol,
            Usuario = usuarioRol.Usuario,
            NombreRol = usuarioRol.NombreRol,
            Activo = usuarioRol.Activo
        };
    }
}
