namespace sgta_backend.Common.Responses;

/// <summary>
/// Formato estándar de respuesta de la API.
/// </summary>
public class ApiResponse<T>
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
