using TravelioDatabaseConnector.Models;

namespace TravelioDatabaseConnector.Models.Carrito;

public class CarritoAutoItem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;

    public string IdAutoProveedor { get; set; } = default!;
    public string Tipo { get; set; } = default!;
    public string? Categoria { get; set; }
    public string? Transmision { get; set; }
    public int CapacidadPasajeros { get; set; }
    public decimal PrecioNormalPorDia { get; set; }
    public decimal PrecioActualPorDia { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public string? UriImagen { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? HoldId { get; set; }
    public DateTime? HoldExpira { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
