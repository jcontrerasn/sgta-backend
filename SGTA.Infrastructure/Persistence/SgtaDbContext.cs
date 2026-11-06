
using Microsoft.EntityFrameworkCore;
using SGTA.Domain.Entities;

namespace SGTA.Infrastructure.Persistence;

public class SgtaDbContext : DbContext
{
    public SgtaDbContext(
        DbContextOptions<SgtaDbContext> options
    ) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles", "dbo");

            entity.HasKey(e => e.IdRol);

            entity.Property(e => e.IdRol)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.Nombre)
                  .HasMaxLength(50)
                  .IsUnicode(false)
                  .IsRequired();

            entity.Property(e => e.Descripcion)
                  .HasMaxLength(200);

            entity.Property(e => e.Activo)
                  .IsRequired();
        });
    }
}
