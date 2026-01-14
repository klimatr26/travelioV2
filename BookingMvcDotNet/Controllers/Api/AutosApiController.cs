using BookingMvcDotNet.Models;
using BookingMvcDotNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingMvcDotNet.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AutosApiController(IAutosService autosService, ILogger<AutosApiController> logger) : ControllerBase
{
    /// <summary>
    /// Buscar autos disponibles con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarAutos(
        [FromQuery] string? ciudad,
        [FromQuery] string? categoria,
        [FromQuery] string? transmision,
        [FromQuery] int? capacidad,
        [FromQuery] decimal? precioMin,
        [FromQuery] decimal? precioMax,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var filtros = new AutosSearchViewModel
            {
                Ciudad = ciudad,
                Categoria = categoria,
                Transmision = transmision,
                Capacidad = capacidad,
                PrecioMin = precioMin,
                PrecioMax = precioMax,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var resultado = await autosService.BuscarAutosAsync(filtros);
            return Ok(new { success = true, data = resultado.Resultados, total = resultado.Resultados?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error buscando autos");
            return StatusCode(500, new { success = false, message = "Error al buscar vehículos" });
        }
    }

    /// <summary>
    /// Obtener detalle de un auto específico
    /// </summary>
    [HttpGet("{servicioId}/{idAuto}")]
    public async Task<IActionResult> ObtenerAuto(int servicioId, string idAuto)
    {
        try
        {
            var auto = await autosService.ObtenerAutoAsync(servicioId, idAuto);
            if (auto == null)
                return NotFound(new { success = false, message = "Vehículo no encontrado" });

            return Ok(new { success = true, data = auto });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo auto {IdAuto}", idAuto);
            return StatusCode(500, new { success = false, message = "Error al obtener vehículo" });
        }
    }

    /// <summary>
    /// Verificar disponibilidad de un auto
    /// </summary>
    [HttpPost("disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad([FromBody] VerificarDisponibilidadRequest request)
    {
        try
        {
            var disponible = await autosService.VerificarDisponibilidadAsync(
                request.ServicioId, request.IdAuto, request.FechaInicio, request.FechaFin);
            return Ok(new { success = true, disponible });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verificando disponibilidad");
            return StatusCode(500, new { success = false, message = "Error verificando disponibilidad" });
        }
    }

    /// <summary>
    /// Diagnóstico: probar conexiones a servicios de autos
    /// </summary>
    [HttpGet("debug")]
    public async Task<IActionResult> Debug()
    {
        var resultados = await autosService.DiagnosticarServiciosAsync();
        return Ok(resultados);
    }
}

public class VerificarDisponibilidadRequest
{
    public int ServicioId { get; set; }
    public string IdAuto { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
