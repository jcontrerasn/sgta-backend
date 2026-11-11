using SGTA.Domain.Entities;

namespace SGTA.Application.Interfaces;

public interface IUsuarioRolRepository
{
    Task<bool> ExisteRolAsync(
        int idRol,
        CancellationToken cancellationToken = default);

    Task<string?> ObtenerNombreRolAsync(
        int idRol,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteAsignacionAsync(
        string usuario,
        int idRol,
        int? idUsuarioRolExcluir = null,
        CancellationToken cancellationToken = default);

    Task<string?> ObtenerNombreRolActivoAsync(
        string usuario,
        CancellationToken cancellationToken = default);

    Task<List<UsuarioRol>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<UsuarioRol> CrearAsync(
        UsuarioRol usuarioRol,
        CancellationToken cancellationToken = default);

    Task<UsuarioRol?> ActualizarAsync(
        UsuarioRol usuarioRol,
        CancellationToken cancellationToken = default);

    Task<UsuarioRol?> CambiarEstadoAsync(
        int idUsuarioRol,
        CancellationToken cancellationToken = default);
}
