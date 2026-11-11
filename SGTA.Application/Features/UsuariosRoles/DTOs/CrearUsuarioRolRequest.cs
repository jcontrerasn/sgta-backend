using System.ComponentModel.DataAnnotations;

namespace SGTA.Application.Features.UsuariosRoles.DTOs;

public class CrearUsuarioRolRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(100)]
    public string Usuario { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El Id del rol debe ser mayor que cero.")]
    public int IdRol { get; set; }
}
