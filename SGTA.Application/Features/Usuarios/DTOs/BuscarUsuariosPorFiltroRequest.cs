using System.ComponentModel.DataAnnotations;

namespace SGTA.Application.Features.Usuarios.DTOs;

public class BuscarUsuariosPorFiltroRequest
{
    [Required(ErrorMessage = "El filtro es obligatorio.")]
    [StringLength(50, ErrorMessage = "El filtro no puede superar los 50 caracteres.")]
    public string Pfiltro_c { get; set; } = string.Empty;
}
