namespace SGTA.Application.Features.Alertas.DTOs;

public class AlertaResponse
{
    public int IdAlerta { get; set; }
    public int IdTarea { get; set; }
    public string TipoAlerta { get; set; } = string.Empty;
    public string TituloTarea { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string UsuarioCreador { get; set; } = string.Empty;
    public DateTime FechaCreacionTarea { get; set; }
    public DateOnly FechaVencimientoTarea { get; set; }
    public string EstadoEnvio { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaEncolada { get; set; }
    public string? ErrorDetalle { get; set; }
    public List<AlertaDestinatarioResponse> Destinatarios { get; set; } = [];
}
