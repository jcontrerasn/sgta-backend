using System.ComponentModel.DataAnnotations;

namespace SGTA.Application.Features.Tareas.DTOs;

public class ListarTareasFlagRequest
{
    [Required(ErrorMessage = "El flag es obligatorio.")]
    [StringLength(20, ErrorMessage = "El flag no puede superar los 20 caracteres.")]
    public string Flag { get; set; } = string.Empty;

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(100, ErrorMessage = "El usuario no puede superar los 100 caracteres.")]
    public string Usuario { get; set; } = string.Empty;
}
