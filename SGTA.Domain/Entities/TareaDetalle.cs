namespace SGTA.Domain.Entities;

public class TareaDetalle
{
    public string Titulo { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string UsuarioCreador { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public List<UsuarioAsignadoDetalle> UsuariosAsignados { get; set; } = [];
    public List<AnotacionTareaDetalle> Anotaciones { get; set; } = [];
}
