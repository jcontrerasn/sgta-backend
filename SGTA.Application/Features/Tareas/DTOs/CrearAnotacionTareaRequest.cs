using System.ComponentModel.DataAnnotations;

namespace SGTA.Application.Features.Tareas.DTOs;

public class CrearAnotacionTareaRequest
{
    [Required(ErrorMessage = "El usuario autor es obligatorio.")]
    [StringLength(100)]
    public string UsuarioAutor { get; set; } = string.Empty;

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    public string Comentario { get; set; } = string.Empty;

    public bool EsComentarioCierre { get; set; }
}
