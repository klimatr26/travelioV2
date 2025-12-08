namespace TravelioDatabaseConnector.Models;

public class Compra
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaCompra { get; set; }
    public decimal ValorPagado { get; set; }
    public string? FacturaUrl { get; set; }

    public ICollection<ReservaCompra> ReservasCompra { get; set; } = new List<ReservaCompra>();
}
