using SGTA.Domain.Entities;

namespace SGTA.Application.Interfaces;

public interface IRolRepository
{
    Task<List<Rol>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistePorNombreAsync(
        string nombre,
        CancellationToken cancellationToken = default);

    Task<Rol> CrearAsync(
        Rol rol,
        CancellationToken cancellationToken = default);
}
