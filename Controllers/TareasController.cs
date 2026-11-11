using Microsoft.AspNetCore.Mvc;
using SGTA.Application.Features.Tareas;
using SGTA.Application.Features.Tareas.DTOs;
using sgta_backend.Common.Responses;

namespace sgta_backend.Controllers;

[ApiController]
[Route("api/tareas")]
/// <summary>
/// Operaciones para gestionar tareas.
/// </summary>
public class TareasController : ControllerBase
{
    private readonly TareaService _tareaService;

    public TareasController(TareaService tareaService)
    {
        _tareaService = tareaService;
    }

    // PUT /api/tareas/{idTarea}
    /// <summary>
    /// Actualiza una tarea y reemplaza sus usuarios involucrados.
    /// </summary>
    /// <param name="idTarea">Identificador de la tarea.</param>
    /// <param name="request">Datos actualizados de la tarea.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Tarea actualizada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No se encontró la tarea indicada.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<TareaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPut("{idTarea:int}")]
    public async Task<IActionResult> Actualizar(
        [FromRoute] int idTarea,
        [FromBody] ActualizarTareaRequest request,
        CancellationToken cancellationToken)
    {
        var tarea = await _tareaService.ActualizarAsync(
            idTarea, request, cancellationToken);

        if (tarea is null)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontró la tarea indicada.",
                Data = null
            });
        }

        return Ok(new ApiResponse<TareaResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Tarea actualizada correctamente.",
            Data = tarea
        });
    }

    // DELETE /api/tareas/{idTarea}?usuario={usuario}
    /// <summary>
    /// Elimina una tarea, sus anotaciones y sus usuarios involucrados.
    /// Solo puede eliminarla el usuario que la creó.
    /// </summary>
    /// <param name="idTarea">Identificador de la tarea.</param>
    /// <param name="usuario">Usuario que solicita la eliminación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Tarea eliminada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No se encontró la tarea o el usuario no es su creador.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpDelete("{idTarea:int}")]
    public async Task<IActionResult> Eliminar(
        [FromRoute] int idTarea,
        [FromQuery] string usuario,
        CancellationToken cancellationToken)
    {
        var eliminado = await _tareaService.EliminarAsync(
            idTarea, usuario, cancellationToken);

        if (!eliminado)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontró la tarea o el usuario no es su creador.",
                Data = null
            });
        }

        return Ok(new ApiResponse<object>
        {
            Status = StatusCodes.Status200OK,
            Message = "Tarea eliminada correctamente.",
            Data = null
        });
    }

    // GET /api/tareas/{idTarea}
    /// <summary>
    /// Obtiene el detalle de una tarea y sus usuarios asignados.
    /// </summary>
    /// <param name="idTarea">Identificador de la tarea.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Detalle de la tarea obtenido correctamente.</response>
    /// <response code="400">El identificador de la tarea no es válido.</response>
    /// <response code="404">No se encontró la tarea indicada.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<TareaDetalleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet("{idTarea:int}")]
    public async Task<IActionResult> ObtenerPorId(
        [FromRoute] int idTarea,
        CancellationToken cancellationToken)
    {
        var tarea = await _tareaService.ObtenerDetallePorIdAsync(
            idTarea, cancellationToken);

        if (tarea is null)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontró la tarea indicada.",
                Data = null
            });
        }

        return Ok(new ApiResponse<TareaDetalleResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Detalle de la tarea obtenido correctamente.",
            Data = tarea
        });
    }

    // GET /api/tareas/{idTarea}/historial
    /// <summary>
    /// Obtiene todo el historial de cambios de una tarea.
    /// </summary>
    /// <param name="idTarea">Identificador de la tarea.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Historial obtenido correctamente.</response>
    /// <response code="400">El identificador de la tarea no es válido.</response>
    /// <response code="404">No se encontró la tarea indicada.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<List<HistorialTareaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet("{idTarea:int}/historial")]
    public async Task<IActionResult> ObtenerHistorial(
        [FromRoute] int idTarea,
        CancellationToken cancellationToken)
    {
        var historial = await _tareaService.ObtenerHistorialAsync(
            idTarea, cancellationToken);

        if (historial is null)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontró la tarea indicada.",
                Data = null
            });
        }

        return Ok(new ApiResponse<List<HistorialTareaResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Historial de la tarea obtenido correctamente.",
            Data = historial
        });
    }

    // POST /api/tareas/{idTarea}/anotaciones
    /// <summary>
    /// Agrega una anotación a una tarea.
    /// </summary>
    /// <param name="idTarea">Identificador de la tarea.</param>
    /// <param name="request">Datos de la anotación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="201">Anotación creada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No se encontró la tarea indicada.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<AnotacionTareaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPost("{idTarea:int}/anotaciones")]
    public async Task<IActionResult> CrearAnotacion(
        [FromRoute] int idTarea,
        [FromBody] CrearAnotacionTareaRequest request,
        CancellationToken cancellationToken)
    {
        var anotacion = await _tareaService.CrearAnotacionAsync(
            idTarea, request, cancellationToken);

        if (anotacion is null)
        {
            return NotFound(new ApiResponse<object>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "No se encontró la tarea indicada.",
                Data = null
            });
        }

        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<AnotacionTareaResponse>
            {
                Status = StatusCodes.Status201Created,
                Message = "Anotación creada correctamente.",
                Data = anotacion
            });
    }

    // GET /api/tareas/creadas-por/{usuarioCreador}
    /// <summary>
    /// Obtiene las tareas creadas por un usuario.
    /// </summary>
    /// <param name="usuarioCreador">Usuario que creó las tareas.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Lista de tareas obtenida correctamente.</response>
    /// <response code="400">El usuario creador no es válido.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<List<TareaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet("creadas-por/{usuarioCreador}")]
    public async Task<IActionResult> ObtenerCreadasPorUsuario(
        [FromRoute] string usuarioCreador,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerCreadasPorUsuarioAsync(
            usuarioCreador, cancellationToken);

        return Ok(new ApiResponse<List<TareaResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Tareas obtenidas correctamente.",
            Data = tareas
        });
    }

    // GET /api/tareas/involucrado/{usuario}
    /// <summary>
    /// Obtiene las tareas donde un usuario está involucrado.
    /// </summary>
    /// <param name="usuario">Usuario involucrado en las tareas.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Lista de tareas obtenida correctamente.</response>
    /// <response code="400">El usuario involucrado no es válido.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<List<TareaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet("involucrado/{usuario}")]
    public async Task<IActionResult> ObtenerComoInvolucrado(
        [FromRoute] string usuario,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerInvolucradoEnAsync(
            usuario, cancellationToken);

        return Ok(new ApiResponse<List<TareaResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Tareas donde el usuario está involucrado obtenidas correctamente.",
            Data = tareas
        });
    }

    // GET /api/tareas/otras/{usuario}
    /// <summary>
    /// Obtiene tareas donde el usuario no es creador ni involucrado,
    /// aplicando las reglas de visibilidad de su rol.
    /// </summary>
    /// <param name="usuario">Usuario que consulta las tareas.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Tareas obtenidas correctamente.</response>
    /// <response code="400">El usuario no está registrado o no tiene un rol activo.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<List<TareaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpGet("otras/{usuario}")]
    public async Task<IActionResult> ObtenerOtras(
        [FromRoute] string usuario,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.ObtenerOtrasAsync(
            usuario, cancellationToken);

        return Ok(new ApiResponse<List<TareaResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Otras tareas obtenidas correctamente.",
            Data = tareas
        });
    }

    /// <summary>
    /// Registra una tarea y sus usuarios involucrados en una sola transacción.
    /// Las anotaciones no se insertan durante este proceso.
    /// </summary>
    /// <param name="request">Datos de la tarea. Los involucrados deben enviarse separados por punto y coma.</param>
    /// <response code="201">Tarea registrada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(ApiResponse<TareaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearTareaRequest request,
        CancellationToken cancellationToken)
    {
        var tarea = await _tareaService.CrearAsync(
            request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<TareaResponse>
            {
                Status = StatusCodes.Status201Created,
                Message = "Tarea registrada correctamente.",
                Data = tarea
            });
    }
}
