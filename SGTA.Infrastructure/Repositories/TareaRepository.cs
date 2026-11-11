using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;
using SGTA.Infrastructure.Persistence;
using System.Data;

namespace SGTA.Infrastructure.Repositories;

public class TareaRepository : ITareaRepository
{
    private readonly SgtaDbContext _context;
    private readonly string _connectionString;

    public TareaRepository(
        SgtaDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration
            .GetConnectionString("SgtaConnection")
            ?? throw new InvalidOperationException(
                "No se configuró ConnectionStrings:SgtaConnection.");
    }

    public async Task<TareaDetalle?> ObtenerDetallePorIdAsync(
        int idTarea,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_trackit_BuscarTareaXId", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(new SqlParameter(
            "PidTarea", SqlDbType.Int)
        {
            Value = idTarea
        });

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var tarea = new TareaDetalle
        {
            Titulo = LeerTexto(reader, "Titulo"),
            FechaVencimiento = DateOnly.FromDateTime(
                reader.GetDateTime(reader.GetOrdinal("FechaVencimiento"))),
            Descripcion = LeerTexto(reader, "Descripcion"),
            UsuarioCreador = LeerTexto(reader, "UsuarioCreador"),
            Estado = LeerTexto(reader, "Estado")
        };

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                tarea.UsuariosAsignados.Add(new UsuarioAsignadoDetalle
                {
                    Usuario = LeerTexto(reader, "Usuario"),
                    NombreCompleto = LeerTexto(reader, "NombreCompleto")
                });
            }
        }

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                tarea.Anotaciones.Add(new AnotacionTareaDetalle
                {
                    IdAnotacion = reader.GetInt32(
                        reader.GetOrdinal("IdAnotacion")),
                    IdTarea = reader.GetInt32(
                        reader.GetOrdinal("IdTarea")),
                    UsuarioAutor = LeerTexto(reader, "UsuarioAutor"),
                    Comentario = LeerTexto(reader, "Comentario"),
                    EsComentarioCierre = reader.GetBoolean(
                        reader.GetOrdinal("EsComentarioCierre")),
                    FechaCreacion = reader.GetDateTime(
                        reader.GetOrdinal("FechaCreacion"))
                });
            }
        }

        return tarea;
    }

    public async Task<TareaAnotacion?> CrearAnotacionAsync(
        TareaAnotacion anotacion,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_trackit_ActualizarTareaAnotaciones", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(new SqlParameter(
            "PidTarea", SqlDbType.Int)
        {
            Value = anotacion.IdTarea
        });

        command.Parameters.Add(new SqlParameter(
            "PusuarioAutor", SqlDbType.VarChar, 100)
        {
            Value = anotacion.UsuarioAutor
        });

        command.Parameters.Add(new SqlParameter(
            "Pcomentario", SqlDbType.NVarChar, -1)
        {
            Value = anotacion.Comentario
        });

        command.Parameters.Add(new SqlParameter(
            "PesComentarioCierre", SqlDbType.Bit)
        {
            Value = anotacion.EsComentarioCierre
        });

        try
        {
            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(
                cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new TareaAnotacion
            {
                IdAnotacion = reader.GetInt32(
                    reader.GetOrdinal("IdAnotacion")),
                IdTarea = reader.GetInt32(
                    reader.GetOrdinal("IdTarea")),
                UsuarioAutor = LeerTexto(reader, "UsuarioAutor"),
                Comentario = LeerTexto(reader, "Comentario"),
                EsComentarioCierre = reader.GetBoolean(
                    reader.GetOrdinal("EsComentarioCierre")),
                FechaCreacion = reader.GetDateTime(
                    reader.GetOrdinal("FechaCreacion"))
            };
        }
        catch (SqlException exception) when (exception.Number == 50022)
        {
            return null;
        }
    }

    public async Task<Tarea?> ActualizarAsync(
        int idTarea,
        Tarea tarea,
        IReadOnlyCollection<string> usuariosInvolucrados,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_trackit_ActualizarTarea", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(new SqlParameter(
            "PidTarea", SqlDbType.Int)
        {
            Value = idTarea
        });

        command.Parameters.Add(new SqlParameter(
            "Ptitulo", SqlDbType.NVarChar, 120)
        {
            Value = tarea.Titulo
        });

        command.Parameters.Add(new SqlParameter(
            "Pdescripcion", SqlDbType.NVarChar, -1)
        {
            Value = tarea.Descripcion
        });

        command.Parameters.Add(new SqlParameter(
            "PfechaVencimiento", SqlDbType.Date)
        {
            Value = tarea.FechaVencimiento.ToDateTime(TimeOnly.MinValue)
        });

        command.Parameters.Add(new SqlParameter(
            "PusuariosInvolucrados", SqlDbType.NVarChar, -1)
        {
            Value = string.Join(';', usuariosInvolucrados)
        });

        try
        {
            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(
                cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            var tareaActualizada = new Tarea
            {
                IdTarea = reader.GetInt32(reader.GetOrdinal("IdTarea")),
                Titulo = LeerTexto(reader, "Titulo"),
                Descripcion = LeerTexto(reader, "Descripcion"),
                FechaVencimiento = DateOnly.FromDateTime(
                    reader.GetDateTime(reader.GetOrdinal("FechaVencimiento"))),
                UsuarioCreador = LeerTexto(reader, "UsuarioCreador"),
                Estado = LeerTexto(reader, "Estado"),
                FechaCreacion = reader.GetDateTime(
                    reader.GetOrdinal("FechaCreacion"))
            };

            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    tareaActualizada.Involucrados.Add(new TareaInvolucrado
                    {
                        IdTareaInvolucrado = reader.GetInt32(
                            reader.GetOrdinal("IdTareaInvolucrado")),
                        IdTarea = reader.GetInt32(
                            reader.GetOrdinal("IdTarea")),
                        Usuario = LeerTexto(reader, "Usuario"),
                        FechaAsignacion = reader.GetDateTime(
                            reader.GetOrdinal("FechaAsignacion"))
                    });
                }
            }

            return tareaActualizada;
        }
        catch (SqlException exception) when (exception.Number == 50017)
        {
            return null;
        }
    }

    public async Task<bool> EliminarAsync(
        int idTarea,
        string usuarioCreador,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_trackit_EliminarTarea", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(new SqlParameter(
            "PidTarea", SqlDbType.Int)
        {
            Value = idTarea
        });

        command.Parameters.Add(new SqlParameter(
            "PusuarioCreador", SqlDbType.VarChar, 100)
        {
            Value = usuarioCreador
        });

        try
        {
            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }
        catch (SqlException exception) when (exception.Number == 50025)
        {
            return false;
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<Tarea>> ObtenerCreadasPorUsuarioAsync(
        string usuarioCreador,
        CancellationToken cancellationToken = default)
    {
        var tareas = await _context.Tareas
            .AsNoTracking()
            .Include(tarea => tarea.Involucrados)
            .Where(tarea => tarea.UsuarioCreador == usuarioCreador)
            .ToListAsync(cancellationToken);

        await CargarRolesCreadoresAsync(tareas, cancellationToken);
        return OrdenarPorVencimientoCercano(tareas);
    }

    public async Task<List<Tarea>> ObtenerInvolucradoEnAsync(
        string usuario,
        CancellationToken cancellationToken = default)
    {
        var tareas = await _context.Tareas
            .AsNoTracking()
            .Include(tarea => tarea.Involucrados)
            .Where(tarea => tarea.Involucrados
                .Any(involucrado => involucrado.Usuario == usuario))
            .ToListAsync(cancellationToken);

        await CargarRolesCreadoresAsync(tareas, cancellationToken);
        return OrdenarPorVencimientoCercano(tareas);
    }

    public async Task<List<Tarea>> ObtenerOtrasAsync(
        string usuario,
        string rolUsuario,
        CancellationToken cancellationToken = default)
    {
        var tareas = _context.Tareas
            .AsNoTracking()
            .Include(tarea => tarea.Involucrados)
            .Where(tarea =>
                tarea.UsuarioCreador != usuario
                && !tarea.Involucrados.Any(
                    involucrado => involucrado.Usuario == usuario));

        if (rolUsuario == "Tecnico_Soporte")
        {
            tareas = tareas.Where(tarea =>
                _context.UsuarioRoles.Any(usuarioRol =>
                    usuarioRol.Usuario == tarea.UsuarioCreador
                    && usuarioRol.Activo
                    && _context.Roles.Any(rol =>
                        rol.IdRol == usuarioRol.IdRol
                        && rol.Activo
                        && (rol.Nombre == "Tecnico_Soporte"
                            || rol.Nombre == "Responsable_HW_SW"))));
        }

        var resultado = await tareas
            .ToListAsync(cancellationToken);

        await CargarRolesCreadoresAsync(resultado, cancellationToken);
        return OrdenarPorVencimientoCercano(resultado);
    }

    public async Task<Tarea> CrearConInvolucradosAsync(
        Tarea tarea,
        IReadOnlyCollection<string> usuariosInvolucrados,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_trackit_InsertarNuevaTarea", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(new SqlParameter(
            "Ptitulo", SqlDbType.NVarChar, 120)
        {
            Value = tarea.Titulo
        });

        command.Parameters.Add(new SqlParameter(
            "Pdescripcion", SqlDbType.NVarChar, -1)
        {
            Value = tarea.Descripcion
        });

        command.Parameters.Add(new SqlParameter(
            "PfechaVencimiento", SqlDbType.Date)
        {
            Value = tarea.FechaVencimiento.ToDateTime(TimeOnly.MinValue)
        });

        command.Parameters.Add(new SqlParameter(
            "PusuarioCreador", SqlDbType.VarChar, 100)
        {
            Value = tarea.UsuarioCreador
        });

        command.Parameters.Add(new SqlParameter(
            "PusuariosInvolucrados", SqlDbType.NVarChar, -1)
        {
            Value = string.Join(';', usuariosInvolucrados)
        });

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "El procedimiento no devolvió la tarea creada.");
        }

        var tareaCreada = new Tarea
        {
            IdTarea = reader.GetInt32(reader.GetOrdinal("IdTarea")),
            Titulo = LeerTexto(reader, "Titulo"),
            Descripcion = LeerTexto(reader, "Descripcion"),
            FechaVencimiento = DateOnly.FromDateTime(
                reader.GetDateTime(reader.GetOrdinal("FechaVencimiento"))),
            UsuarioCreador = LeerTexto(reader, "UsuarioCreador"),
            Estado = LeerTexto(reader, "Estado"),
            FechaCreacion = reader.GetDateTime(
                reader.GetOrdinal("FechaCreacion"))
        };

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                tareaCreada.Involucrados.Add(new TareaInvolucrado
                {
                    IdTareaInvolucrado = reader.GetInt32(
                        reader.GetOrdinal("IdTareaInvolucrado")),
                    IdTarea = reader.GetInt32(
                        reader.GetOrdinal("IdTarea")),
                    Usuario = LeerTexto(reader, "Usuario"),
                    FechaAsignacion = reader.GetDateTime(
                        reader.GetOrdinal("FechaAsignacion"))
                });
            }
        }

        return tareaCreada;
    }

    private static string LeerTexto(
        SqlDataReader reader,
        string columna)
    {
        var valor = reader[columna];

        return valor == DBNull.Value
            ? string.Empty
            : Convert.ToString(valor) ?? string.Empty;
    }

    private static List<Tarea> OrdenarPorVencimientoCercano(
        List<Tarea> tareas)
    {
        return tareas
            .OrderBy(tarea => tarea.FechaVencimiento)
            .ThenByDescending(tarea => tarea.FechaCreacion)
            .ThenByDescending(tarea => tarea.IdTarea)
            .ToList();
    }

    private async Task CargarRolesCreadoresAsync(
        List<Tarea> tareas,
        CancellationToken cancellationToken)
    {
        var usuarios = tareas
            .Select(tarea => tarea.UsuarioCreador)
            .Where(usuario => !string.IsNullOrWhiteSpace(usuario))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (usuarios.Count == 0)
        {
            return;
        }

        var roles = await (
            from usuarioRol in _context.UsuarioRoles.AsNoTracking()
            join rol in _context.Roles.AsNoTracking()
                on usuarioRol.IdRol equals rol.IdRol
            where usuarios.Contains(usuarioRol.Usuario)
                && usuarioRol.Activo
                && rol.Activo
            select new
            {
                usuarioRol.Usuario,
                Rol = rol.Nombre
            })
            .ToListAsync(cancellationToken);

        var rolesPorUsuario = roles
            .GroupBy(item => item.Usuario, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.First().Rol,
                StringComparer.OrdinalIgnoreCase);

        foreach (var tarea in tareas)
        {
            if (rolesPorUsuario.TryGetValue(
                tarea.UsuarioCreador, out var rol))
            {
                tarea.RolUsuarioCreador = rol;
            }
        }
    }
}
