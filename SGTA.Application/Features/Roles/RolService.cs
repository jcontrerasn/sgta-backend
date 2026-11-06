using SGTA.Application.Interfaces;
using SGTA.Application.Exceptions;
using SGTA.Domain.Entities;
using SGTA.Application.Features.Roles.DTOs;

namespace SGTA.Application.Features.Roles;

public class RolService
{
    private readonly IRolRepository _repository;

    public RolService(IRolRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RolResponse>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await _repository.ObtenerTodosAsync(
            cancellationToken);

        return roles.Select(rol => new RolResponse
        {
            IdRol = rol.IdRol,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Activo = rol.Activo
        }).ToList();
    }

    public async Task<RolResponse> CrearAsync(
        CrearRolRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new ArgumentException(
                "El nombre del rol es obligatorio");
        }

        var nombre = request.Nombre.Trim();

        if (await _repository.ExistePorNombreAsync(
            nombre, cancellationToken))
        {
            throw new ConflictException(
                "Ya existe un rol con ese nombre.");
        }

        var rol = new Rol
        {
            Nombre = nombre,
            Descripcion = request.Descripcion,
            Activo = request.Activo
        };

        var rolCreado = await _repository.CrearAsync(
            rol, cancellationToken);

        return new RolResponse
        {
            IdRol = rolCreado.IdRol,
            Nombre = rolCreado.Nombre,
            Descripcion = rolCreado.Descripcion,
            Activo = rolCreado.Activo
        };
    }
}
