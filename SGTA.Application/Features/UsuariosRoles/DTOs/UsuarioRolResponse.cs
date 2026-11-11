namespace SGTA.Application.Features.UsuariosRoles.DTOs;

public class UsuarioRolResponse
{
    public int IdUsuarioRol { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string NombreRol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
