using BookingMvcDotNet.Models;
using BookingMvcDotNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingMvcDotNet.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class HotelesApiController(IHotelesService hotelesService, ILogger<HotelesApiController> logger) : ControllerBase
{
    /// <summary>
    /// Buscar habitaciones disponibles con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarHabitaciones(
        [FromQuery] string? ciudad,
        [FromQuery] string? tipoHabitacion,
        [FromQuery] int? capacidad,
        [FromQuery] decimal? precioMin,
        [FromQuery] decimal? precioMax,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] int numeroHuespedes = 2)
    {
        try
        {
            var filtros = new HabitacionesSearchViewModel
            {
                Ciudad = ciudad,
                TipoHabitacion = tipoHabitacion,
                Capacidad = capacidad,
                PrecioMin = precioMin,
                PrecioMax = precioMax,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                NumeroHuespedes = numeroHuespedes
            };

            var resultado = await hotelesService.BuscarHabitacionesAsync(filtros);
            return Ok(new { success = true, data = resultado.Resultados, total = resultado.Resultados?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error buscando habitaciones");
            return StatusCode(500, new { success = false, message = "Error al buscar habitaciones" });
        }
    }

    /// <summary>
    /// Obtener detalle de una habitación específica
    /// </summary>
    [HttpGet("{servicioId}/{idHabitacion}")]
    public async Task<IActionResult> ObtenerHabitacion(int servicioId, string idHabitacion)
    {
        try
        {
            var habitacion = await hotelesService.ObtenerHabitacionAsync(servicioId, idHabitacion);
            if (habitacion == null)
                return NotFound(new { success = false, message = "Habitación no encontrada" });

            return Ok(new { success = true, data = habitacion });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo habitación {IdHabitacion}", idHabitacion);
            return StatusCode(500, new { success = false, message = "Error al obtener habitación" });
        }
    }

    /// <summary>
    /// Verificar disponibilidad de una habitación
    /// </summary>
    [HttpPost("disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad([FromBody] VerificarDisponibilidadHabitacionRequest request)
    {
        try
        {
            var disponible = await hotelesService.VerificarDisponibilidadAsync(
                request.ServicioId, request.IdHabitacion, request.FechaInicio, request.FechaFin);
            return Ok(new { success = true, disponible });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verificando disponibilidad");
            return StatusCode(500, new { success = false, message = "Error verificando disponibilidad" });
        }
    }
}

public class VerificarDisponibilidadHabitacionRequest
{
    public int ServicioId { get; set; }
    public string IdHabitacion { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
