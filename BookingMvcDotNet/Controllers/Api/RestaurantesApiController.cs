using BookingMvcDotNet.Models;
using BookingMvcDotNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingMvcDotNet.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class RestaurantesApiController(IRestaurantesService restaurantesService, ILogger<RestaurantesApiController> logger) : ControllerBase
{
    /// <summary>
    /// Buscar mesas disponibles con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarMesas(
        [FromQuery] int? capacidad,
        [FromQuery] string? tipoMesa,
        [FromQuery] DateTime? fecha,
        [FromQuery] int numeroPersonas = 2)
    {
        try
        {
            var filtros = new MesasSearchViewModel
            {
                Capacidad = capacidad,
                TipoMesa = tipoMesa,
                Fecha = fecha,
                NumeroPersonas = numeroPersonas
            };

            var resultado = await restaurantesService.BuscarMesasAsync(filtros);
            return Ok(new { success = true, data = resultado.Resultados, total = resultado.Resultados?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error buscando mesas");
            return StatusCode(500, new { success = false, message = "Error al buscar mesas" });
        }
    }

    /// <summary>
    /// Obtener detalle de una mesa específica
    /// </summary>
    [HttpGet("{servicioId}/{idMesa:int}")]
    public async Task<IActionResult> ObtenerMesa(int servicioId, int idMesa)
    {
        try
        {
            var mesa = await restaurantesService.ObtenerMesaAsync(servicioId, idMesa);
            if (mesa == null)
                return NotFound(new { success = false, message = "Mesa no encontrada" });

            return Ok(new { success = true, data = mesa });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo mesa {IdMesa}", idMesa);
            return StatusCode(500, new { success = false, message = "Error al obtener mesa" });
        }
    }

    /// <summary>
    /// Verificar disponibilidad de una mesa
    /// </summary>
    [HttpPost("disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad([FromBody] VerificarDisponibilidadMesaRequest request)
    {
        try
        {
            var disponible = await restaurantesService.VerificarDisponibilidadAsync(
                request.ServicioId, request.IdMesa, request.Fecha, request.Personas);
            return Ok(new { success = true, disponible });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verificando disponibilidad");
            return StatusCode(500, new { success = false, message = "Error verificando disponibilidad" });
        }
    }
}

public class VerificarDisponibilidadMesaRequest
{
    public int ServicioId { get; set; }
    public int IdMesa { get; set; }
    public DateTime Fecha { get; set; }
    public int Personas { get; set; } = 2;
}
