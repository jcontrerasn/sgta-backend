namespace SGTA.Domain.Entities;

public class TareaInvolucrado
{
    public int IdTareaInvolucrado { get; set; }
    public int IdTarea { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }

    public Tarea? Tarea { get; set; }
}
