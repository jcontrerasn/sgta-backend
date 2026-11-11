using System.ComponentModel.DataAnnotations;

namespace SGTA.Application.Features.Tareas.DTOs;

public class CrearTareaRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(120)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    public DateOnly FechaVencimiento { get; set; }

    [Required(ErrorMessage = "El usuario creador es obligatorio.")]
    [StringLength(100)]
    public string UsuarioCreador { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe indicar al menos un usuario involucrado.")]
    public string UsuariosInvolucrados { get; set; } = string.Empty;
}
