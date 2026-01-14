using BookingMvcDotNet.Models;
using BookingMvcDotNet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelioDatabaseConnector.Data;
using TravelioDatabaseConnector.Enums;
using TravelioREST.Aerolinea;

namespace BookingMvcDotNet.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class VuelosApiController(IVuelosService vuelosService, ILogger<VuelosApiController> logger, TravelioDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Endpoint de debug para ver servicios de aerolineas configurados
    /// </summary>
    [HttpGet("debug/servicios")]
    public async Task<IActionResult> DebugServicios()
    {
        var servicios = await dbContext.Servicios
            .Where(s => s.TipoServicio == TipoServicio.Aerolinea)
            .ToListAsync();

        var servicioIds = servicios.Select(s => s.Id).ToList();
        var detalles = await dbContext.DetallesServicio
            .Where(d => servicioIds.Contains(d.ServicioId))
            .ToListAsync();

        var resultado = servicios.Select(s => new
        {
            s.Id,
            s.Nombre,
            s.TipoServicio,
            s.Activo,
            Detalles = detalles
                .Where(d => d.ServicioId == s.Id)
                .Select(d => new
                {
                    d.TipoProtocolo,
                    d.UriBase,
                    d.ObtenerProductosEndpoint,
                    UrlCompleta = $"{d.UriBase}{d.ObtenerProductosEndpoint}"
                })
                .ToList()
        }).ToList();

        return Ok(new { success = true, servicios = resultado, total = servicios.Count });
    }

    /// <summary>
    /// Endpoint de debug para probar REST directamente
    /// </summary>
    [HttpGet("debug/test-rest")]
    public async Task<IActionResult> DebugTestRest()
    {
        var resultados = new List<object>();
        
        var servicios = await dbContext.Servicios
            .Where(s => s.TipoServicio == TipoServicio.Aerolinea && s.Activo)
            .ToListAsync();

        var servicioIds = servicios.Select(s => s.Id).ToList();
        var detalles = await dbContext.DetallesServicio
            .Where(d => servicioIds.Contains(d.ServicioId) && d.TipoProtocolo == TipoProtocolo.Rest)
            .ToListAsync();

        foreach (var servicio in servicios)
        {
            var detalle = detalles.FirstOrDefault(d => d.ServicioId == servicio.Id);
            if (detalle == null)
            {
                resultados.Add(new { servicio = servicio.Nombre, error = "No tiene REST configurado" });
                continue;
            }

            var url = $"{detalle.UriBase}{detalle.ObtenerProductosEndpoint}";
            try
            {
                logger.LogInformation("DEBUG: Llamando a {Url}", url);
                var response = await VuelosGetter.GetVuelosAsync(url);
                resultados.Add(new 
                { 
                    servicio = servicio.Nombre, 
                    url,
                    success = true, 
                    count = response?.Length ?? 0,
                    primerVuelo = response?.FirstOrDefault()?.IdVuelo
                });
            }
            catch (Exception ex)
            {
                resultados.Add(new 
                { 
                    servicio = servicio.Nombre, 
                    url,
                    success = false, 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(3).ToArray()
                });
            }
        }

        return Ok(new { resultados });
    }

    /// <summary>
    /// Buscar vuelos disponibles con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarVuelos(
        [FromQuery] string? origen,
        [FromQuery] string? destino,
        [FromQuery] DateTime? fechaSalida,
        [FromQuery] string? tipoCabina,
        [FromQuery] int pasajeros = 1,
        [FromQuery] decimal? precioMin = null,
        [FromQuery] decimal? precioMax = null)
    {
        try
        {
            var filtros = new VuelosSearchViewModel
            {
                Origen = origen,
                Destino = destino,
                FechaSalida = fechaSalida,
                TipoCabina = tipoCabina,
                Pasajeros = pasajeros,
                PrecioMin = precioMin,
                PrecioMax = precioMax
            };

            var resultado = await vuelosService.BuscarVuelosAsync(filtros);
            return Ok(new { success = true, data = resultado.Resultados, total = resultado.Resultados?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error buscando vuelos");
            return StatusCode(500, new { success = false, message = "Error al buscar vuelos" });
        }
    }

    /// <summary>
    /// Obtener detalle de un vuelo específico
    /// </summary>
    [HttpGet("{servicioId}/{idVuelo}")]
    public async Task<IActionResult> ObtenerVuelo(int servicioId, string idVuelo)
    {
        try
        {
            var vuelo = await vuelosService.ObtenerVueloAsync(servicioId, idVuelo);
            if (vuelo == null)
                return NotFound(new { success = false, message = "Vuelo no encontrado" });

            return Ok(new { success = true, data = vuelo });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo vuelo {IdVuelo}", idVuelo);
            return StatusCode(500, new { success = false, message = "Error al obtener vuelo" });
        }
    }

    /// <summary>
    /// Verificar disponibilidad de asientos
    /// </summary>
    [HttpPost("disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad([FromBody] VerificarDisponibilidadVueloRequest request)
    {
        try
        {
            var disponible = await vuelosService.VerificarDisponibilidadAsync(
                request.ServicioId, request.IdVuelo, request.Pasajeros);
            return Ok(new { success = true, disponible });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verificando disponibilidad");
            return StatusCode(500, new { success = false, message = "Error verificando disponibilidad" });
        }
    }
}

public class VerificarDisponibilidadVueloRequest
{
    public int ServicioId { get; set; }
    public string IdVuelo { get; set; } = "";
    public int Pasajeros { get; set; } = 1;
}
