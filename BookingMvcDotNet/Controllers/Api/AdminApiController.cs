using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelioDatabaseConnector.Data;
using TravelioDatabaseConnector.Enums;

namespace BookingMvcDotNet.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AdminApiController(TravelioDbContext dbContext, ILogger<AdminApiController> logger) : ControllerBase
{
    /// <summary>
    /// Obtener estadísticas del dashboard
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var totalClientes = await dbContext.Clientes.CountAsync();
            var totalCompras = await dbContext.Compras.CountAsync();
            var totalReservas = await dbContext.Reservas.CountAsync();
            var totalProveedores = await dbContext.Servicios.CountAsync();
            
            var ingresosMes = await dbContext.Compras
                .Where(c => c.FechaCompra.Month == DateTime.Now.Month && c.FechaCompra.Year == DateTime.Now.Year)
                .SumAsync(c => c.ValorPagado);

            var reservasActivas = await dbContext.Reservas
                .Where(r => r.Activa)
                .CountAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalClientes,
                    totalCompras,
                    totalReservas,
                    totalProveedores,
                    ingresosMes,
                    reservasPendientes = reservasActivas
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo stats");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    /// <summary>
    /// Obtener lista de clientes
    /// </summary>
    [HttpGet("clientes")]
    public async Task<IActionResult> GetClientes([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var query = dbContext.Clientes.AsQueryable();
            var total = await query.CountAsync();
            
            var clientes = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(c => new
                {
                    id = c.Id,
                    nombres = c.Nombre,
                    apellidos = c.Apellido,
                    email = c.CorreoElectronico,
                    documento = c.DocumentoIdentidad,
                    telefono = c.Telefono,
                    pais = c.Pais,
                    estado = "Activo"
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = clientes,
                total,
                page,
                totalPages = (int)Math.Ceiling(total / (double)limit)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo clientes");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    /// <summary>
    /// Obtener lista de compras
    /// </summary>
    [HttpGet("compras")]
    public async Task<IActionResult> GetCompras([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var query = dbContext.Compras.Include(c => c.Cliente).AsQueryable();
            var total = await query.CountAsync();
            
            var compras = await query
                .OrderByDescending(c => c.FechaCompra)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(c => new
                {
                    id = c.Id,
                    cliente = c.Cliente != null ? $"{c.Cliente.Nombre} {c.Cliente.Apellido}" : "N/A",
                    clienteEmail = c.Cliente != null ? c.Cliente.CorreoElectronico : "",
                    fecha = c.FechaCompra,
                    subtotal = c.ValorPagado / 1.12m,
                    iva = c.ValorPagado - (c.ValorPagado / 1.12m),
                    total = c.ValorPagado,
                    estado = "Completada"
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = compras,
                total,
                page,
                totalPages = (int)Math.Ceiling(total / (double)limit)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo compras");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    /// <summary>
    /// Obtener lista de reservas
    /// </summary>
    [HttpGet("reservas")]
    public async Task<IActionResult> GetReservas([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var query = dbContext.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.ReservasCompra)
                    .ThenInclude(rc => rc.Compra)
                        .ThenInclude(c => c.Cliente)
                .AsQueryable();
            
            var total = await query.CountAsync();
            
            var reservas = await query
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(r => new
                {
                    id = r.Id,
                    tipo = r.Servicio != null ? GetTipoServicioLabel(r.Servicio.TipoServicio) : "N/A",
                    servicio = r.Servicio != null ? r.Servicio.Nombre : "N/A",
                    cliente = r.ReservasCompra.FirstOrDefault() != null && r.ReservasCompra.First().Compra.Cliente != null 
                        ? $"{r.ReservasCompra.First().Compra.Cliente.Nombre} {r.ReservasCompra.First().Compra.Cliente.Apellido}" 
                        : "N/A",
                    fecha = r.ReservasCompra.FirstOrDefault() != null ? r.ReservasCompra.First().Compra.FechaCompra : DateTime.MinValue,
                    fechaServicio = "Ver detalle",
                    estado = r.Activa ? "Activa" : "Cancelada",
                    codigoReserva = r.CodigoReserva
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = reservas,
                total,
                page,
                totalPages = (int)Math.Ceiling(total / (double)limit)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo reservas");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    /// <summary>
    /// Obtener lista de proveedores/servicios
    /// </summary>
    [HttpGet("proveedores")]
    public async Task<IActionResult> GetProveedores([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var query = dbContext.Servicios.AsQueryable();
            var total = await query.CountAsync();
            
            var proveedores = await query
                .OrderBy(s => s.TipoServicio)
                .ThenBy(s => s.Nombre)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(s => new
                {
                    id = s.Id,
                    nombre = s.Nombre,
                    tipo = GetTipoServicioLabel(s.TipoServicio),
                    tipoServicio = s.TipoServicio.ToString(),
                    activo = s.Activo,
                    estado = s.Activo ? "Activo" : "Inactivo"
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = proveedores,
                total,
                page,
                totalPages = (int)Math.Ceiling(total / (double)limit)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo proveedores");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    /// <summary>
    /// Activar/Desactivar proveedor
    /// </summary>
    [HttpPut("proveedores/{id}/toggle")]
    public async Task<IActionResult> ToggleProveedor(int id)
    {
        try
        {
            var servicio = await dbContext.Servicios.FindAsync(id);
            if (servicio == null)
            {
                return NotFound(new { success = false, message = "Proveedor no encontrado" });
            }

            servicio.Activo = !servicio.Activo;
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Proveedor {(servicio.Activo ? "activado" : "desactivado")}",
                activo = servicio.Activo
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error actualizando proveedor");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    /// <summary>
    /// Actividad reciente (últimas transacciones)
    /// </summary>
    [HttpGet("actividad-reciente")]
    public async Task<IActionResult> GetActividadReciente([FromQuery] int limit = 10)
    {
        try
        {
            // Obtener reservas recientes
            var reservasRecientes = await dbContext.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.ReservasCompra)
                    .ThenInclude(rc => rc.Compra)
                        .ThenInclude(c => c.Cliente)
                .OrderByDescending(r => r.Id)
                .Take(limit / 2)
                .ToListAsync();

            var actividadReservas = reservasRecientes.Select(r => new
            {
                tipo = "Reserva",
                descripcion = r.Servicio != null ? $"Reserva en {r.Servicio.Nombre}" : "Nueva reserva",
                cliente = r.ReservasCompra.FirstOrDefault()?.Compra?.Cliente != null 
                    ? $"{r.ReservasCompra.First().Compra.Cliente.Nombre} {r.ReservasCompra.First().Compra.Cliente.Apellido}" 
                    : "N/A",
                fecha = r.ReservasCompra.FirstOrDefault()?.Compra?.FechaCompra ?? DateTime.MinValue,
                estado = r.Activa ? "Activa" : "Cancelada"
            }).ToList();

            // Obtener compras recientes
            var comprasRecientes = await dbContext.Compras
                .Include(c => c.Cliente)
                .OrderByDescending(c => c.FechaCompra)
                .Take(limit / 2)
                .Select(c => new
                {
                    tipo = "Compra",
                    descripcion = $"Compra por ${c.ValorPagado:F2}",
                    cliente = c.Cliente != null ? $"{c.Cliente.Nombre} {c.Cliente.Apellido}" : "N/A",
                    fecha = c.FechaCompra,
                    estado = "Completada"
                })
                .ToListAsync();

            // Combinar y ordenar por fecha
            var actividad = actividadReservas
                .Cast<object>()
                .Concat(comprasRecientes.Cast<object>())
                .OrderByDescending(a => ((dynamic)a).fecha)
                .Take(limit)
                .ToList();

            return Ok(new
            {
                success = true,
                data = actividad
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo actividad reciente");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    private static string GetTipoServicioLabel(TipoServicio tipo) => tipo switch
    {
        TipoServicio.Aerolinea => "Aerolínea",
        TipoServicio.Hotel => "Hotel",
        TipoServicio.Restaurante => "Restaurante",
        TipoServicio.RentaVehiculos => "Alquiler de Autos",
        TipoServicio.PaquetesTuristicos => "Paquete Turístico",
        _ => tipo.ToString()
    };
}
