namespace SGTA.Application.Features.Tareas.DTOs;

public class TareaDetalleResponse
{
    public string Titulo { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string UsuarioCreador { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public List<UsuarioAsignadoResponse> UsuariosAsignados { get; set; } = [];
    public List<AnotacionTareaResponse> Anotaciones { get; set; } = [];
}
