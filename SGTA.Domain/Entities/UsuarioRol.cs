namespace SGTA.Domain.Entities;

public class UsuarioRol
{
    public int IdUsuarioRol { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
