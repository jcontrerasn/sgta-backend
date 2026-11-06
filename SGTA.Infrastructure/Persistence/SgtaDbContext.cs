
using Microsoft.EntityFrameworkCore;

namespace SGTA.Infrastructure.Persistence;

public class SgtaDbContext : DbContext
{
    public SgtaDbContext(
        DbContextOptions<SgtaDbContext> options
    ) : base(options)
    {
    }

    // Aquí agregaremos nuestras entidades
    // Ejemplo:
    // public DbSet<Usuario> Usuarios => Set<Usuario>();
}
