namespace SGTA.Application.Features.Tareas.DTOs;

public class HistorialTareaResponse
{
    public int IdHistorial { get; set; }
    public int IdTarea { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public string? Descripcion { get; set; }
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public int? IdAnotacion { get; set; }
    public string? CampoModificado { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
}
