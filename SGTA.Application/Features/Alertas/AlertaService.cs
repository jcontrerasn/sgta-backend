using SGTA.Application.Features.Alertas.DTOs;
using SGTA.Application.Interfaces;

namespace SGTA.Application.Features.Alertas;

public class AlertaService
{
    private readonly IAlertaRepository _repository;

    public AlertaService(IAlertaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AlertaResponse>> ObtenerHistorialAsync(
        CancellationToken cancellationToken = default)
    {
        var alertas = await _repository.ObtenerHistorialAsync(
            cancellationToken);

        return alertas.Select(alerta => new AlertaResponse
        {
            IdAlerta = alerta.IdAlerta,
            IdTarea = alerta.IdTarea,
            TipoAlerta = alerta.TipoAlerta,
            TituloTarea = alerta.TituloTarea,
            Asunto = alerta.Asunto,
            UsuarioCreador = alerta.UsuarioCreador,
            FechaCreacionTarea = alerta.FechaCreacionTarea,
            FechaVencimientoTarea = alerta.FechaVencimientoTarea,
            EstadoEnvio = alerta.EstadoEnvio,
            FechaCreacion = alerta.FechaCreacion,
            FechaEncolada = alerta.FechaEncolada,
            ErrorDetalle = alerta.ErrorDetalle,
            Destinatarios = alerta.Destinatarios
                .OrderBy(destinatario => destinatario.IdDestinatario)
                .Select(destinatario => new AlertaDestinatarioResponse
                {
                    IdDestinatario = destinatario.IdDestinatario,
                    IdAlerta = destinatario.IdAlerta,
                    Usuario = destinatario.Usuario,
                    Correo = destinatario.Correo,
                    TipoDestinatario = destinatario.TipoDestinatario,
                    EstadoEnvio = destinatario.EstadoEnvio,
                    FechaEncolada = destinatario.FechaEncolada,
                    ErrorDetalle = destinatario.ErrorDetalle
                })
                .ToList()
        }).ToList();
    }
}
