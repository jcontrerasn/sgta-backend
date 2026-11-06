using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SGTA.Application.Exceptions;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;
using SGTA.Infrastructure.Persistence;

namespace SGTA.Infrastructure.Repositories;

public class RolRepository : IRolRepository
{
    private readonly SgtaDbContext _context;

    public RolRepository(SgtaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rol>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorNombreAsync(
        string nombre,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .AnyAsync(rol => rol.Nombre == nombre, cancellationToken);
    }

    public async Task<Rol> CrearAsync(
        Rol rol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Roles.AddAsync(rol, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            throw new ConflictException(
                "Ya existe un rol con ese nombre.", ex);
        }

        return rol;
    }

    private static bool EsViolacionDeUnicidad(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
