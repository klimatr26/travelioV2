using TravelioDatabaseConnector.Models.Carrito;

namespace TravelioIntegrator.Models.Carrito;

public record struct AerolineaPasajeroRequest(
    string Nombre,
    string Apellido,
    string TipoIdentificacion,
    string Identificacion,
    DateOnly FechaNacimiento);

public record struct AerolineaCarritoRequest(
    int ClienteId,
    int ServicioId,
    string IdVueloProveedor,
    string Origen,
    string Destino,
    DateTime FechaVuelo,
    string TipoCabina,
    string NombreAerolinea,
    decimal PrecioNormal,
    decimal PrecioActual,
    decimal DescuentoPorcentaje,
    int CantidadPasajeros,
    IReadOnlyCollection<AerolineaPasajeroRequest> Pasajeros);
