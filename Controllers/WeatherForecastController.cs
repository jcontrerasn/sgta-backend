using Microsoft.AspNetCore.Mvc;

namespace sgta_backend.Controllers;

[ApiController]
[Route("[controller]")]
/// <summary>
/// Endpoint de ejemplo generado por la plantilla de ASP.NET Core.
/// </summary>
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// Obtiene un pronóstico de clima de ejemplo.
    /// </summary>
    /// <response code="200">Pronóstico generado correctamente.</response>
    [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), StatusCodes.Status200OK)]
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
