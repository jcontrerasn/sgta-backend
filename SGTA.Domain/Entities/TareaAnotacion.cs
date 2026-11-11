namespace SGTA.Domain.Entities;

public class TareaAnotacion
{
    public int IdAnotacion { get; set; }
    public int IdTarea { get; set; }
    public string UsuarioAutor { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public bool EsComentarioCierre { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Tarea? Tarea { get; set; }
}
