using TravelioDatabaseConnector.Models;

namespace TravelioDatabaseConnector.Models.Carrito;

public class CarritoHabitacionItem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;

    public string IdHabitacionProveedor { get; set; } = default!;
    public string NombreHabitacion { get; set; } = default!;
    public string TipoHabitacion { get; set; } = default!;
    public string Hotel { get; set; } = default!;
    public string Ciudad { get; set; } = default!;
    public string Pais { get; set; } = default!;
    public int Capacidad { get; set; }
    public decimal PrecioNormal { get; set; }
    public decimal PrecioActual { get; set; }
    public decimal PrecioVigente { get; set; }
    public string Amenidades { get; set; } = string.Empty;
    public string? Imagenes { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int NumeroHuespedes { get; set; }
    public string? HoldId { get; set; }
    public DateTime? HoldExpira { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
