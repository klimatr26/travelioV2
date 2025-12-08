using TravelioDatabaseConnector.Models;

namespace TravelioDatabaseConnector.Models.Carrito;

public class CarritoMesaItem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;

    public int IdMesa { get; set; }
    public int IdRestaurante { get; set; }
    public int NumeroMesa { get; set; }
    public string TipoMesa { get; set; } = default!;
    public int Capacidad { get; set; }
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public string EstadoMesa { get; set; } = default!;
    public DateTime FechaReserva { get; set; }
    public int NumeroPersonas { get; set; }
    public string? HoldId { get; set; }
    public DateTime? HoldExpira { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
