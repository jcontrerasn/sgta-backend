using System.ComponentModel.DataAnnotations;

namespace SGTA.Application.Features.Tareas.DTOs;

public class ListarTareasFlagRequest
{
    [Range(1, 3, ErrorMessage = "El flag debe ser 1, 2 o 3.")]
    public int Flag { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(100, ErrorMessage = "El usuario no puede superar los 100 caracteres.")]
    public string Usuario { get; set; } = string.Empty;
}
