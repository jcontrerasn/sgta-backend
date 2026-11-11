namespace SGTA.Application.Features.Tareas.DTOs;

public class TareaResponse
{
    public int IdTarea { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public string UsuarioCreador { get; set; } = string.Empty;
    public string RolUsuarioCreador { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<string> UsuariosInvolucrados { get; set; } = [];
}
