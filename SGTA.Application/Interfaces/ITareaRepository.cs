using SGTA.Domain.Entities;

namespace SGTA.Application.Interfaces;

public interface ITareaRepository
{
    Task<TareaDetalle?> ObtenerDetallePorIdAsync(
        int idTarea,
        CancellationToken cancellationToken = default);

    Task<TareaAnotacion?> CrearAnotacionAsync(
        TareaAnotacion anotacion,
        CancellationToken cancellationToken = default);

    Task<Tarea?> ActualizarAsync(
        int idTarea,
        Tarea tarea,
        IReadOnlyCollection<string> usuariosInvolucrados,
        CancellationToken cancellationToken = default);

    Task<bool> EliminarAsync(
        int idTarea,
        string usuarioCreador,
        CancellationToken cancellationToken = default);

    Task<List<Tarea>> ObtenerCreadasPorUsuarioAsync(
        string usuarioCreador,
        CancellationToken cancellationToken = default);

    Task<List<Tarea>> ObtenerInvolucradoEnAsync(
        string usuario,
        CancellationToken cancellationToken = default);

    Task<List<Tarea>> ObtenerOtrasAsync(
        string usuario,
        string rolUsuario,
        CancellationToken cancellationToken = default);

    Task<Tarea> CrearConInvolucradosAsync(
        Tarea tarea,
        IReadOnlyCollection<string> usuariosInvolucrados,
        CancellationToken cancellationToken = default);
}
