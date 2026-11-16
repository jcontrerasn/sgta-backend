namespace SGTA.Application.Features.Alertas.DTOs;

public class AlertaDestinatarioResponse
{
    public int IdDestinatario { get; set; }
    public int IdAlerta { get; set; }
    public string? Usuario { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string TipoDestinatario { get; set; } = string.Empty;
    public string EstadoEnvio { get; set; } = string.Empty;
    public DateTime? FechaEncolada { get; set; }
    public string? ErrorDetalle { get; set; }
}
