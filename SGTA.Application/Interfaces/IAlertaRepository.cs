using SGTA.Domain.Entities;

namespace SGTA.Application.Interfaces;

public interface IAlertaRepository
{
    Task<List<Alerta>> ObtenerHistorialAsync(
        CancellationToken cancellationToken = default);
}
