using TravelioDatabaseConnector.Models;

namespace TravelioDatabaseConnector.Models.Carrito;

public class CarritoAerolineaItem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;

    public string IdVueloProveedor { get; set; } = default!;
    public string Origen { get; set; } = default!;
    public string Destino { get; set; } = default!;
    public DateTime FechaVuelo { get; set; }
    public string TipoCabina { get; set; } = default!;
    public string NombreAerolinea { get; set; } = default!;
    public decimal PrecioNormal { get; set; }
    public decimal PrecioActual { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public int CantidadPasajeros { get; set; }
    public string? HoldId { get; set; }
    public DateTime? HoldExpira { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<CarritoAerolineaPasajero> Pasajeros { get; set; } = new List<CarritoAerolineaPasajero>();
}
