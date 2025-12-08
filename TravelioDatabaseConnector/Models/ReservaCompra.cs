namespace TravelioDatabaseConnector.Models;

public class ReservaCompra
{
    public int CompraId { get; set; }
    public Compra Compra { get; set; } = null!;
    public int ReservaId { get; set; }
    public Reserva Reserva { get; set; } = null!;
}
