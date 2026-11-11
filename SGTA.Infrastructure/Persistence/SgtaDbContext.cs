
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
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<TareaInvolucrado> TareaInvolucrados => Set<TareaInvolucrado>();
    public DbSet<TareaAnotacion> TareaAnotaciones => Set<TareaAnotacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("TRACKIT_Roles", "dbo");

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

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("TRACKIT_UsuarioRoles", "dbo");

            entity.HasKey(e => e.IdUsuarioRol);

            entity.Property(e => e.IdUsuarioRol)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.Usuario)
                  .HasMaxLength(100)
                  .IsUnicode(false)
                  .IsRequired();

            entity.Property(e => e.IdRol)
                  .IsRequired();

            entity.Ignore(e => e.NombreRol);

            entity.Property(e => e.Activo)
                  .IsRequired();

            entity.HasOne<Rol>()
                  .WithMany()
                  .HasForeignKey(e => e.IdRol)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Tarea>(entity =>
        {
            entity.ToTable("TRACKIT_Tareas", "dbo");

            entity.HasKey(e => e.IdTarea);

            entity.Property(e => e.IdTarea)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.Titulo)
                  .HasMaxLength(120)
                  .IsRequired();

            entity.Property(e => e.Descripcion)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired();

            entity.Property(e => e.FechaVencimiento)
                  .HasColumnType("date")
                  .IsRequired();

            entity.Property(e => e.UsuarioCreador)
                  .HasMaxLength(100)
                  .IsUnicode(false)
                  .IsRequired();

            entity.Property(e => e.FechaCreacion)
                  .HasColumnType("datetime2(0)")
                  .HasDefaultValueSql("GETDATE()")
                  .IsRequired();

            entity.Property(e => e.FechaModificacion)
                  .HasColumnType("datetime2(0)");

            entity.Property(e => e.FechaCierre)
                  .HasColumnType("datetime2(0)");

            entity.Property(e => e.UsuarioCierre)
                  .HasMaxLength(100)
                  .IsUnicode(false);

            entity.Property(e => e.FechaEliminacion)
                  .HasColumnType("datetime2(0)");

            entity.Property(e => e.UsuarioEliminacion)
                  .HasMaxLength(100)
                  .IsUnicode(false);

            entity.Property(e => e.Estado)
                  .HasMaxLength(20)
                  .IsUnicode(false);

            entity.Ignore(e => e.RolUsuarioCreador);
        });

        modelBuilder.Entity<TareaInvolucrado>(entity =>
        {
            entity.ToTable("TRACKIT_TareaInvolucrados", "dbo");

            entity.HasKey(e => e.IdTareaInvolucrado);

            entity.Property(e => e.IdTareaInvolucrado)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.IdTarea)
                  .IsRequired();

            entity.Property(e => e.Usuario)
                  .HasMaxLength(100)
                  .IsUnicode(false)
                  .IsRequired();

            entity.Property(e => e.FechaAsignacion)
                  .HasColumnType("datetime2(0)")
                  .HasDefaultValueSql("GETDATE()")
                  .IsRequired();

            entity.HasIndex(e => new { e.IdTarea, e.Usuario })
                  .IsUnique();

            entity.HasOne(e => e.Tarea)
                  .WithMany(e => e.Involucrados)
                  .HasForeignKey(e => e.IdTarea)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TareaAnotacion>(entity =>
        {
            entity.ToTable("TRACKIT_TareaAnotaciones", "dbo");

            entity.HasKey(e => e.IdAnotacion);

            entity.Property(e => e.IdAnotacion)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.IdTarea)
                  .IsRequired();

            entity.Property(e => e.UsuarioAutor)
                  .HasMaxLength(100)
                  .IsUnicode(false)
                  .IsRequired();

            entity.Property(e => e.Comentario)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired();

            entity.Property(e => e.EsComentarioCierre)
                  .HasDefaultValue(false)
                  .IsRequired();

            entity.Property(e => e.FechaCreacion)
                  .HasColumnType("datetime2(0)")
                  .HasDefaultValueSql("GETDATE()")
                  .IsRequired();

            entity.HasOne(e => e.Tarea)
                  .WithMany()
                  .HasForeignKey(e => e.IdTarea)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
