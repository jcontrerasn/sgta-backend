using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGTA.Application.Exceptions;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;
using SGTA.Infrastructure.Persistence;

namespace SGTA.Infrastructure.Repositories;

public class UsuarioRolRepository : IUsuarioRolRepository
{
    private readonly SgtaDbContext _context;

    public UsuarioRolRepository(SgtaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteRolAsync(
        int idRol,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .AnyAsync(rol => rol.IdRol == idRol, cancellationToken);
    }

    public async Task<string?> ObtenerNombreRolAsync(
        int idRol,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(rol => rol.IdRol == idRol)
            .Select(rol => rol.Nombre)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsignacionAsync(
        string usuario,
        int idRol,
        int? idUsuarioRolExcluir = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.UsuarioRoles
            .AsNoTracking()
            .AnyAsync(usuarioRol =>
                usuarioRol.Usuario == usuario
                && usuarioRol.IdRol == idRol
                && (!idUsuarioRolExcluir.HasValue
                    || usuarioRol.IdUsuarioRol != idUsuarioRolExcluir.Value),
                cancellationToken);
    }

    public async Task<string?> ObtenerNombreRolActivoAsync(
        string usuario,
        CancellationToken cancellationToken = default)
    {
        return await (
            from usuarioRol in _context.UsuarioRoles.AsNoTracking()
            join rol in _context.Roles.AsNoTracking()
                on usuarioRol.IdRol equals rol.IdRol
            where usuarioRol.Usuario == usuario
                && usuarioRol.Activo
            select rol.Nombre)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UsuarioRol>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await (
            from usuarioRol in _context.UsuarioRoles.AsNoTracking()
            join rol in _context.Roles.AsNoTracking()
                on usuarioRol.IdRol equals rol.IdRol
            orderby usuarioRol.IdUsuarioRol
            select new UsuarioRol
            {
                IdUsuarioRol = usuarioRol.IdUsuarioRol,
                Usuario = usuarioRol.Usuario,
                IdRol = usuarioRol.IdRol,
                NombreRol = rol.Nombre,
                Activo = usuarioRol.Activo
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioRol> CrearAsync(
        UsuarioRol usuarioRol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.UsuarioRoles.AddAsync(
                usuarioRol, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (EsViolacionDeUnicidad(exception))
        {
            throw new ConflictException(
                "Ya existe una asignación para ese usuario y rol.",
                exception);
        }

        return usuarioRol;
    }

    public async Task<UsuarioRol?> ActualizarAsync(
        UsuarioRol usuarioRol,
        CancellationToken cancellationToken = default)
    {
        var existente = await _context.UsuarioRoles
            .FirstOrDefaultAsync(
                asignacion => asignacion.IdUsuarioRol
                    == usuarioRol.IdUsuarioRol,
                cancellationToken);

        if (existente is null)
        {
            return null;
        }

        existente.Usuario = usuarioRol.Usuario;
        existente.IdRol = usuarioRol.IdRol;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (EsViolacionDeUnicidad(exception))
        {
            throw new ConflictException(
                "Ya existe una asignación para ese usuario y rol.",
                exception);
        }

        return existente;
    }

    public async Task<UsuarioRol?> CambiarEstadoAsync(
        int idUsuarioRol,
        CancellationToken cancellationToken = default)
    {
        var existente = await _context.UsuarioRoles
            .FirstOrDefaultAsync(
                asignacion => asignacion.IdUsuarioRol == idUsuarioRol,
                cancellationToken);

        if (existente is null)
        {
            return null;
        }

        existente.Activo = !existente.Activo;
        await _context.SaveChangesAsync(cancellationToken);

        return existente;
    }

    public async Task<bool> EliminarAsync(
        int idUsuarioRol,
        CancellationToken cancellationToken = default)
    {
        var asignacion = await _context.UsuarioRoles
            .FirstOrDefaultAsync(
                registro => registro.IdUsuarioRol == idUsuarioRol,
                cancellationToken);

        if (asignacion is null)
        {
            return false;
        }

        _context.UsuarioRoles.Remove(asignacion);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static bool EsViolacionDeUnicidad(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
