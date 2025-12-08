namespace TravelioDatabaseConnector.Models.Carrito;

public class CarritoPaqueteTurista
{
    public int Id { get; set; }

    public int CarritoPaqueteItemId { get; set; }
    public CarritoPaqueteItem Carrito { get; set; } = null!;

    public string Nombre { get; set; } = default!;
    public string Apellido { get; set; } = default!;
    public DateOnly? FechaNacimiento { get; set; }
    public string TipoIdentificacion { get; set; } = default!;
    public string Identificacion { get; set; } = default!;
}
