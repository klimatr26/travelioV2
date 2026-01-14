using BookingMvcDotNet.Models;
using BookingMvcDotNet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelioDatabaseConnector.Data;
using TravelioDatabaseConnector.Enums;
using TravelioREST.Paquetes;

namespace BookingMvcDotNet.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PaquetesApiController(IPaquetesService paquetesService, ILogger<PaquetesApiController> logger, TravelioDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Endpoint de debug para ver servicios de paquetes configurados
    /// </summary>
    [HttpGet("debug/servicios")]
    public async Task<IActionResult> DebugServicios()
    {
        var servicios = await dbContext.Servicios
            .Where(s => s.TipoServicio == TipoServicio.PaquetesTuristicos)
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
            .Where(s => s.TipoServicio == TipoServicio.PaquetesTuristicos && s.Activo)
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
                var response = await PaquetesList.ObtenerPaquetesAsync(url);
                resultados.Add(new 
                { 
                    servicio = servicio.Nombre, 
                    url,
                    success = true, 
                    count = response?.datos?.Length ?? 0,
                    primerPaquete = response?.datos?.FirstOrDefault()?.idPaquete
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
    /// Buscar paquetes turísticos disponibles con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BuscarPaquetes(
        [FromQuery] string? ciudad,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] string? tipoActividad,
        [FromQuery] decimal? precioMax,
        [FromQuery] int personas = 1)
    {
        try
        {
            var filtros = new PaquetesSearchViewModel
            {
                Ciudad = ciudad,
                FechaInicio = fechaInicio,
                TipoActividad = tipoActividad,
                PrecioMax = precioMax,
                Personas = personas
            };

            var resultado = await paquetesService.BuscarPaquetesAsync(filtros);
            return Ok(new { success = true, data = resultado.Resultados, total = resultado.Resultados?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error buscando paquetes");
            return StatusCode(500, new { success = false, message = "Error al buscar paquetes" });
        }
    }

    /// <summary>
    /// Obtener detalle de un paquete específico
    /// </summary>
    [HttpGet("{servicioId}/{idPaquete}")]
    public async Task<IActionResult> ObtenerPaquete(int servicioId, string idPaquete)
    {
        try
        {
            var paquete = await paquetesService.ObtenerPaqueteAsync(servicioId, idPaquete);
            if (paquete == null)
                return NotFound(new { success = false, message = "Paquete no encontrado" });

            return Ok(new { success = true, data = paquete });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo paquete {IdPaquete}", idPaquete);
            return StatusCode(500, new { success = false, message = "Error al obtener paquete" });
        }
    }

    /// <summary>
    /// Verificar disponibilidad de un paquete
    /// </summary>
    [HttpPost("disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad([FromBody] VerificarDisponibilidadPaqueteRequest request)
    {
        try
        {
            var disponible = await paquetesService.VerificarDisponibilidadAsync(
                request.ServicioId, request.IdPaquete, request.FechaInicio, request.Personas);
            return Ok(new { success = true, disponible });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verificando disponibilidad");
            return StatusCode(500, new { success = false, message = "Error verificando disponibilidad" });
        }
    }
}

public class VerificarDisponibilidadPaqueteRequest
{
    public int ServicioId { get; set; }
    public string IdPaquete { get; set; } = "";
    public DateTime FechaInicio { get; set; }
    public int Personas { get; set; } = 1;
}
