using SGTA.Application.Features.Tareas.DTOs;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;

namespace SGTA.Application.Features.Tareas;

public class TareaService
{
    private readonly ITareaRepository _repository;
    private readonly IUsuarioRolRepository _usuarioRolRepository;

    public TareaService(
        ITareaRepository repository,
        IUsuarioRolRepository usuarioRolRepository)
    {
        _repository = repository;
        _usuarioRolRepository = usuarioRolRepository;
    }

    public async Task<TareaDetalleResponse?> ObtenerDetallePorIdAsync(
        int idTarea,
        CancellationToken cancellationToken = default)
    {
        if (idTarea <= 0)
        {
            throw new ArgumentException(
                "El Id de la tarea debe ser mayor que cero.");
        }

        var tarea = await _repository.ObtenerDetallePorIdAsync(
            idTarea, cancellationToken);

        if (tarea is null)
        {
            return null;
        }

        return new TareaDetalleResponse
        {
            Titulo = tarea.Titulo,
            FechaVencimiento = tarea.FechaVencimiento,
            Descripcion = tarea.Descripcion,
            UsuarioCreador = tarea.UsuarioCreador,
            Estado = tarea.Estado,
            UsuariosAsignados = tarea.UsuariosAsignados
                .Select(usuario => new UsuarioAsignadoResponse
                {
                    Usuario = usuario.Usuario,
                    NombreCompleto = usuario.NombreCompleto
                })
                .ToList(),
            Anotaciones = tarea.Anotaciones
                .Select(anotacion => new AnotacionTareaResponse
                {
                    IdAnotacion = anotacion.IdAnotacion,
                    IdTarea = anotacion.IdTarea,
                    UsuarioAutor = anotacion.UsuarioAutor,
                    Comentario = anotacion.Comentario,
                    EsComentarioCierre = anotacion.EsComentarioCierre,
                    FechaCreacion = anotacion.FechaCreacion
                })
                .ToList()
        };
    }

    public async Task<AnotacionTareaResponse?> CrearAnotacionAsync(
        int idTarea,
        CrearAnotacionTareaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (idTarea <= 0)
        {
            throw new ArgumentException(
                "El Id de la tarea debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(request.UsuarioAutor))
        {
            throw new ArgumentException(
                "El usuario autor es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Comentario))
        {
            throw new ArgumentException(
                "El comentario es obligatorio.");
        }

        var anotacion = new TareaAnotacion
        {
            IdTarea = idTarea,
            UsuarioAutor = request.UsuarioAutor.Trim(),
            Comentario = request.Comentario.Trim(),
            EsComentarioCierre = request.EsComentarioCierre
        };

        var anotacionCreada = await _repository.CrearAnotacionAsync(
            anotacion, cancellationToken);

        if (anotacionCreada is null)
        {
            return null;
        }

        return new AnotacionTareaResponse
        {
            IdAnotacion = anotacionCreada.IdAnotacion,
            IdTarea = anotacionCreada.IdTarea,
            UsuarioAutor = anotacionCreada.UsuarioAutor,
            Comentario = anotacionCreada.Comentario,
            EsComentarioCierre = anotacionCreada.EsComentarioCierre,
            FechaCreacion = anotacionCreada.FechaCreacion
        };
    }

    public async Task<TareaResponse?> ActualizarAsync(
        int idTarea,
        ActualizarTareaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (idTarea <= 0)
        {
            throw new ArgumentException(
                "El Id de la tarea debe ser mayor que cero.");
        }

        if (request.FechaVencimiento == default)
        {
            throw new ArgumentException(
                "La fecha de vencimiento es obligatoria.");
        }

        var usuariosInvolucrados = (request.UsuariosInvolucrados ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(usuario => !string.IsNullOrWhiteSpace(usuario))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (usuariosInvolucrados.Count == 0)
        {
            throw new ArgumentException(
                "Debe indicar al menos un usuario involucrado.");
        }

        if (usuariosInvolucrados.Any(usuario => usuario.Length > 100))
        {
            throw new ArgumentException(
                "El usuario involucrado no puede superar los 100 caracteres.");
        }

        var tarea = new Tarea
        {
            IdTarea = idTarea,
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            FechaVencimiento = request.FechaVencimiento
        };

        var tareaActualizada = await _repository.ActualizarAsync(
            idTarea,
            tarea,
            usuariosInvolucrados,
            cancellationToken);

        if (tareaActualizada is null)
        {
            return null;
        }

        return new TareaResponse
        {
            IdTarea = tareaActualizada.IdTarea,
            Titulo = tareaActualizada.Titulo,
            Descripcion = tareaActualizada.Descripcion,
            FechaVencimiento = tareaActualizada.FechaVencimiento,
            UsuarioCreador = tareaActualizada.UsuarioCreador,
            RolUsuarioCreador = tareaActualizada.RolUsuarioCreador,
            Estado = tareaActualizada.Estado,
            FechaCreacion = tareaActualizada.FechaCreacion,
            UsuariosInvolucrados = tareaActualizada.Involucrados
                .OrderBy(involucrado => involucrado.IdTareaInvolucrado)
                .Select(involucrado => involucrado.Usuario)
                .ToList()
        };
    }

    public async Task<bool> EliminarAsync(
        int idTarea,
        string usuarioSolicitante,
        CancellationToken cancellationToken = default)
    {
        if (idTarea <= 0)
        {
            throw new ArgumentException(
                "El Id de la tarea debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(usuarioSolicitante))
        {
            throw new ArgumentException(
                "El usuario solicitante es obligatorio.");
        }

        return await _repository.EliminarAsync(
            idTarea,
            usuarioSolicitante.Trim(),
            cancellationToken);
    }

    public async Task<List<TareaResponse>> ObtenerCreadasPorUsuarioAsync(
        string usuarioCreador,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(usuarioCreador))
        {
            throw new ArgumentException(
                "El usuario creador es obligatorio.");
        }

        var tareas = await _repository.ObtenerCreadasPorUsuarioAsync(
            usuarioCreador.Trim(), cancellationToken);

        return tareas.Select(tarea => new TareaResponse
        {
            IdTarea = tarea.IdTarea,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            FechaVencimiento = tarea.FechaVencimiento,
            UsuarioCreador = tarea.UsuarioCreador,
            RolUsuarioCreador = tarea.RolUsuarioCreador,
            Estado = tarea.Estado,
            FechaCreacion = tarea.FechaCreacion,
            UsuariosInvolucrados = tarea.Involucrados
                .OrderBy(involucrado => involucrado.IdTareaInvolucrado)
                .Select(involucrado => involucrado.Usuario)
                .ToList()
        }).ToList();
    }

    public async Task<List<TareaResponse>> ObtenerInvolucradoEnAsync(
        string usuario,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(usuario))
        {
            throw new ArgumentException(
                "El usuario involucrado es obligatorio.");
        }

        var tareas = await _repository.ObtenerInvolucradoEnAsync(
            usuario.Trim(), cancellationToken);

        return tareas.Select(tarea => new TareaResponse
        {
            IdTarea = tarea.IdTarea,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            FechaVencimiento = tarea.FechaVencimiento,
            UsuarioCreador = tarea.UsuarioCreador,
            RolUsuarioCreador = tarea.RolUsuarioCreador,
            Estado = tarea.Estado,
            FechaCreacion = tarea.FechaCreacion,
            UsuariosInvolucrados = tarea.Involucrados
                .OrderBy(involucrado => involucrado.IdTareaInvolucrado)
                .Select(involucrado => involucrado.Usuario)
                .ToList()
        }).ToList();
    }

    public async Task<List<TareaResponse>> ObtenerOtrasAsync(
        string usuario,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(usuario))
        {
            throw new ArgumentException(
                "El usuario es obligatorio.");
        }

        var usuarioNormalizado = usuario.Trim();
        var rolUsuario = await _usuarioRolRepository
            .ObtenerNombreRolActivoAsync(
                usuarioNormalizado, cancellationToken);

        if (string.IsNullOrWhiteSpace(rolUsuario))
        {
            throw new ArgumentException(
                "El usuario no está registrado o no tiene un rol activo en el sistema.");
        }

        var tareas = await _repository.ObtenerOtrasAsync(
            usuarioNormalizado, rolUsuario, cancellationToken);

        return tareas.Select(tarea => new TareaResponse
        {
            IdTarea = tarea.IdTarea,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            FechaVencimiento = tarea.FechaVencimiento,
            UsuarioCreador = tarea.UsuarioCreador,
            RolUsuarioCreador = tarea.RolUsuarioCreador,
            Estado = tarea.Estado,
            FechaCreacion = tarea.FechaCreacion,
            UsuariosInvolucrados = tarea.Involucrados
                .OrderBy(involucrado => involucrado.IdTareaInvolucrado)
                .Select(involucrado => involucrado.Usuario)
                .ToList()
        }).ToList();
    }

    public async Task<TareaResponse> CrearAsync(
        CrearTareaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.FechaVencimiento == default)
        {
            throw new ArgumentException(
                "La fecha de vencimiento es obligatoria.");
        }

        var usuariosInvolucrados = (request.UsuariosInvolucrados ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(usuario => !string.IsNullOrWhiteSpace(usuario))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (usuariosInvolucrados.Count == 0)
        {
            throw new ArgumentException(
                "Debe indicar al menos un usuario involucrado.");
        }

        if (usuariosInvolucrados.Any(usuario => usuario.Length > 100))
        {
            throw new ArgumentException(
                "El usuario involucrado no puede superar los 100 caracteres.");
        }

        var tarea = new Tarea
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            FechaVencimiento = request.FechaVencimiento,
            UsuarioCreador = request.UsuarioCreador.Trim()
        };

        var tareaCreada = await _repository.CrearConInvolucradosAsync(
            tarea,
            usuariosInvolucrados,
            cancellationToken);

        return new TareaResponse
        {
            IdTarea = tareaCreada.IdTarea,
            Titulo = tareaCreada.Titulo,
            Descripcion = tareaCreada.Descripcion,
            FechaVencimiento = tareaCreada.FechaVencimiento,
            UsuarioCreador = tareaCreada.UsuarioCreador,
            Estado = tareaCreada.Estado,
            FechaCreacion = tareaCreada.FechaCreacion,
            UsuariosInvolucrados = usuariosInvolucrados
        };
    }
}
