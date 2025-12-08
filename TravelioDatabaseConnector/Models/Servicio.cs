using TravelioDatabaseConnector.Models.Carrito;
using TravelioDatabaseConnector.Enums;

namespace TravelioDatabaseConnector.Models;

public class Servicio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public TipoServicio TipoServicio { get; set; }
    public string NumeroCuenta { get; set; } = default!;
    public bool Activo { get; set; }

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    // Cambiado de DetalleServicio? a ICollection para soportar múltiples detalles (SOAP y REST)
    public ICollection<DetalleServicio> DetallesServicio { get; set; } = new List<DetalleServicio>();
    public ICollection<CarritoAerolineaItem> CarritoAerolineaItems { get; set; } = new List<CarritoAerolineaItem>();
    public ICollection<CarritoHabitacionItem> CarritoHabitacionItems { get; set; } = new List<CarritoHabitacionItem>();
    public ICollection<CarritoAutoItem> CarritoAutoItems { get; set; } = new List<CarritoAutoItem>();
    public ICollection<CarritoPaqueteItem> CarritoPaqueteItems { get; set; } = new List<CarritoPaqueteItem>();
    public ICollection<CarritoMesaItem> CarritoMesaItems { get; set; } = new List<CarritoMesaItem>();
}
