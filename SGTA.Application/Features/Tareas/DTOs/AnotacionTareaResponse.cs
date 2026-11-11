namespace SGTA.Application.Features.Tareas.DTOs;

public class AnotacionTareaResponse
{
    public int IdAnotacion { get; set; }
    public int IdTarea { get; set; }
    public string UsuarioAutor { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public bool EsComentarioCierre { get; set; }
    public DateTime FechaCreacion { get; set; }
}
