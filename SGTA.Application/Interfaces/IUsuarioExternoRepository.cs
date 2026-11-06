using SGTA.Domain.Entities;

namespace SGTA.Application.Interfaces;

public interface IUsuarioExternoRepository
{
    Task<UsuarioExterno?> BuscarPorCuentaAsync(
        string cuenta,
        CancellationToken cancellationToken = default);
}
