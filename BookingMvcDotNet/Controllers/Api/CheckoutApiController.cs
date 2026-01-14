using Microsoft.AspNetCore.Mvc;
using BookingMvcDotNet.Models;
using BookingMvcDotNet.Services;
using Microsoft.EntityFrameworkCore;
using TravelioDatabaseConnector.Data;

namespace BookingMvcDotNet.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CheckoutApiController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly TravelioDbContext _dbContext;
    private readonly ILogger<CheckoutApiController> _logger;

    public CheckoutApiController(
        ICheckoutService checkoutService,
        TravelioDbContext dbContext,
        ILogger<CheckoutApiController> logger)
    {
        _checkoutService = checkoutService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Procesar el checkout/pago del carrito
    /// </summary>
    [HttpPost("procesar")]
    public async Task<IActionResult> ProcesarCheckout([FromBody] CheckoutRequestDto request)
    {
        try
        {
            if (request == null || request.ClienteId <= 0)
            {
                return BadRequest(new { success = false, message = "Datos de checkout inválidos" });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { success = false, message = "El carrito está vacío" });
            }

            // Convertir items del request a CartItemViewModel
            var cartItems = request.Items.Select(item => new CartItemViewModel
            {
                Tipo = item.Tipo ?? "",
                Titulo = item.Titulo ?? "",
                Detalle = item.Detalle ?? "",
                Cantidad = item.Cantidad,
                PrecioOriginal = item.PrecioOriginal,
                PrecioFinal = item.PrecioFinal,
                PrecioUnitario = item.PrecioFinal,
                ServicioId = item.ServicioId,
                IdProducto = item.IdProducto ?? "",
                FechaInicio = item.FechaInicio,
                FechaFin = item.FechaFin,
                NumeroPersonas = item.NumeroPersonas,
                ImagenUrl = item.ImagenUrl
            }).ToList();

            var datosFacturacion = new DatosFacturacion
            {
                NombreCompleto = request.NombreCompleto ?? "",
                TipoDocumento = request.TipoDocumento ?? "",
                NumeroDocumento = request.NumeroDocumento ?? "",
                Correo = request.Correo ?? ""
            };

            _logger.LogInformation("Procesando checkout para cliente {ClienteId} con {ItemCount} items", 
                request.ClienteId, cartItems.Count);

            var resultado = await _checkoutService.ProcesarCheckoutAsync(
                request.ClienteId,
                request.NumeroCuentaBancaria,
                cartItems,
                datosFacturacion
            );

            if (resultado.Exitoso)
            {
                return Ok(new
                {
                    success = true,
                    message = resultado.Mensaje,
                    data = new
                    {
                        compraId = resultado.CompraId,
                        totalPagado = resultado.TotalPagado,
                        reservas = resultado.Reservas.Select(r => new
                        {
                            tipo = r.Tipo,
                            titulo = r.Titulo,
                            exitoso = r.Exitoso,
                            codigoConfirmacion = r.CodigoReserva,
                            error = r.Error
                        })
                    }
                });
            }
            else
            {
                return Ok(new
                {
                    success = false,
                    message = resultado.Mensaje,
                    data = new
                    {
                        reservas = resultado.Reservas?.Select(r => new
                        {
                            tipo = r.Tipo,
                            titulo = r.Titulo,
                            exitoso = r.Exitoso,
                            error = r.Error
                        })
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando checkout");
            return StatusCode(500, new { success = false, message = "Error interno al procesar el pago" });
        }
    }

    /// <summary>
    /// Cancelar una reserva
    /// </summary>
    [HttpPost("cancelar/{reservaId}")]
    public async Task<IActionResult> CancelarReserva(int reservaId, [FromBody] CancelacionRequestDto request)
    {
        try
        {
            if (request == null || request.ClienteId <= 0)
            {
                return BadRequest(new { success = false, message = "Datos de cancelación inválidos" });
            }

            var resultado = await _checkoutService.CancelarReservaAsync(
                reservaId,
                request.ClienteId,
                request.NumeroCuentaBancaria
            );

            return Ok(new
            {
                success = resultado.Exitoso,
                message = resultado.Mensaje,
                data = new
                {
                    montoReembolsado = resultado.MontoReembolsado
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelando reserva {ReservaId}", reservaId);
            return StatusCode(500, new { success = false, message = "Error interno al cancelar la reserva" });
        }
    }

    /// <summary>
    /// Obtener historial de compras de un cliente
    /// </summary>
    [HttpGet("historial/{clienteId}")]
    public async Task<IActionResult> GetHistorialCompras(int clienteId)
    {
        try
        {
            var compras = await _dbContext.Compras
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaCompra)
                .Select(c => new
                {
                    id = c.Id,
                    fecha = c.FechaCompra,
                    total = c.ValorPagado,
                    estado = "Completada"
                })
                .ToListAsync();

            return Ok(new { success = true, data = compras });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo historial de cliente {ClienteId}", clienteId);
            return StatusCode(500, new { success = false, message = "Error obteniendo historial" });
        }
    }

    /// <summary>
    /// Obtener reservas activas de un cliente
    /// </summary>
    [HttpGet("reservas/{clienteId}")]
    public async Task<IActionResult> GetReservasCliente(int clienteId)
    {
        try
        {
            // Obtener reservas del cliente a través de la relación Compra -> ReservaCompra -> Reserva
            var reservas = await _dbContext.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.ReservasCompra)
                    .ThenInclude(rc => rc.Compra)
                .Where(r => r.Activa && r.ReservasCompra.Any(rc => rc.Compra.ClienteId == clienteId))
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    id = r.Id,
                    tipo = r.Servicio != null ? r.Servicio.TipoServicio.ToString() : "SERVICIO",
                    servicio = r.Servicio != null ? r.Servicio.Nombre : "N/A",
                    fechaReserva = r.ReservasCompra.FirstOrDefault() != null ? r.ReservasCompra.First().Compra.FechaCompra : DateTime.MinValue,
                    fechaInicio = (DateTime?)null,
                    fechaFin = (DateTime?)null,
                    codigoConfirmacion = r.CodigoReserva,
                    precioTotal = r.ValorPagadoNegocio + r.ComisionAgencia,
                    estado = r.Activa ? "Activa" : "Cancelada",
                    facturaProveedorUrl = r.FacturaUrl,
                    compraId = r.ReservasCompra.FirstOrDefault() != null ? r.ReservasCompra.First().CompraId : (int?)null
                })
                .ToListAsync();

            return Ok(new { success = true, data = reservas });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo reservas de cliente {ClienteId}", clienteId);
            return StatusCode(500, new { success = false, message = "Error obteniendo reservas" });
        }
    }

    /// <summary>
    /// Generar factura de Travelio para una compra
    /// </summary>
    [HttpGet("factura-travelio/{compraId}")]
    public async Task<IActionResult> GetFacturaTravelio(int compraId)
    {
        try
        {
            var compra = await _dbContext.Compras
                .Include(c => c.Cliente)
                .Include(c => c.ReservasCompra)
                    .ThenInclude(rc => rc.Reserva)
                        .ThenInclude(r => r.Servicio)
                .FirstOrDefaultAsync(c => c.Id == compraId);

            if (compra == null)
            {
                return NotFound(new { success = false, message = "Compra no encontrada" });
            }

            var items = compra.ReservasCompra.Select(rc => new
            {
                descripcion = rc.Reserva.Servicio?.Nombre ?? "Servicio",
                tipo = rc.Reserva.Servicio?.TipoServicio.ToString() ?? "SERVICIO",
                codigoReserva = rc.Reserva.CodigoReserva,
                cantidad = 1,
                precioUnitario = rc.Reserva.ValorPagadoNegocio + rc.Reserva.ComisionAgencia
            }).ToList();

            var subtotal = items.Sum(i => i.precioUnitario);
            var iva = subtotal * 0.12m;
            var total = subtotal + iva;

            var factura = new
            {
                compraId = compra.Id,
                numeroFactura = $"TRV-{compra.Id:D6}",
                fechaEmision = compra.FechaCompra,
                cliente = new
                {
                    nombre = $"{compra.Cliente.Nombre} {compra.Cliente.Apellido}",
                    tipoDocumento = compra.Cliente.TipoIdentificacion,
                    documento = compra.Cliente.DocumentoIdentidad,
                    correo = compra.Cliente.CorreoElectronico
                },
                items,
                subtotal,
                iva,
                porcentajeIva = 12m,
                total,
                metodoPago = "Transferencia Bancaria",
                estadoPago = "Pagado"
            };

            return Ok(new { success = true, data = factura });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando factura Travelio para compra {CompraId}", compraId);
            return StatusCode(500, new { success = false, message = "Error generando factura" });
        }
    }
}

/// <summary>
/// DTO para la solicitud de checkout
/// </summary>
public class CheckoutRequestDto
{
    public int ClienteId { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string TipoDocumento { get; set; } = "";
    public string NumeroDocumento { get; set; } = "";
    public string Correo { get; set; } = "";
    public int NumeroCuentaBancaria { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO para un item del carrito
/// </summary>
public class CartItemDto
{
    public string? Tipo { get; set; }
    public string? Titulo { get; set; }
    public string? Detalle { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioOriginal { get; set; }
    public decimal PrecioFinal { get; set; }
    public int ServicioId { get; set; }
    public string? IdProducto { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? NumeroPersonas { get; set; }
    public string? ImagenUrl { get; set; }
}

/// <summary>
/// DTO para cancelación de reserva
/// </summary>
public class CancelacionRequestDto
{
    public int ClienteId { get; set; }
    public int NumeroCuentaBancaria { get; set; }
}
