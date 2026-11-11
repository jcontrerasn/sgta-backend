using SGTA.Domain.Entities;

namespace SGTA.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorCuentaAsync(
        string cuenta,
        CancellationToken cancellationToken = default);

    Task<List<Usuario>> BuscarPorFiltroAsync(
        string filtro,
        CancellationToken cancellationToken = default);
}
