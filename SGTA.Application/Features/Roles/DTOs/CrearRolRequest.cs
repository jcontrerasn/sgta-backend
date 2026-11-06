using System.ComponentModel.DataAnnotations;

namespace SGTA.Application.Features.Roles;

public class CrearRolRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}