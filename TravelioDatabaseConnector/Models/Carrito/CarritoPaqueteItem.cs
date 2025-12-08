using TravelioDatabaseConnector.Models;

namespace TravelioDatabaseConnector.Models.Carrito;

public class CarritoPaqueteItem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;

    public string IdPaqueteProveedor { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Ciudad { get; set; } = default!;
    public string Pais { get; set; } = default!;
    public string TipoActividad { get; set; } = default!;
    public int Capacidad { get; set; }
    public decimal PrecioNormal { get; set; }
    public decimal PrecioActual { get; set; }
    public string? ImagenUrl { get; set; }
    public int Duracion { get; set; }
    public DateTime FechaInicio { get; set; }
    public int Personas { get; set; }
    public string BookingUserId { get; set; } = default!;
    public string? HoldId { get; set; }
    public DateTime? HoldExpira { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<CarritoPaqueteTurista> Turistas { get; set; } = new List<CarritoPaqueteTurista>();
}
