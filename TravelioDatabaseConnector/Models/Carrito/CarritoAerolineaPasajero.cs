namespace TravelioDatabaseConnector.Models.Carrito;

public class CarritoAerolineaPasajero
{
    public int Id { get; set; }

    public int CarritoAerolineaItemId { get; set; }
    public CarritoAerolineaItem Carrito { get; set; } = null!;

    public string Nombre { get; set; } = default!;
    public string Apellido { get; set; } = default!;
    public string TipoIdentificacion { get; set; } = default!;
    public string Identificacion { get; set; } = default!;
    public DateOnly FechaNacimiento { get; set; }
}
