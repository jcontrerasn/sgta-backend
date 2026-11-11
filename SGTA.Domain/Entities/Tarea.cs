namespace SGTA.Domain.Entities;

public class Tarea
{
    public int IdTarea { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public string UsuarioCreador { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string? UsuarioCierre { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    public string? UsuarioEliminacion { get; set; }
    public string? Estado { get; set; }
    public string RolUsuarioCreador { get; set; } = string.Empty;

    public ICollection<TareaInvolucrado> Involucrados { get; set; } = [];
}
