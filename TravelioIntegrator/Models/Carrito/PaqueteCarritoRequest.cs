namespace TravelioIntegrator.Models.Carrito;

public record struct PaqueteTuristaRequest(
    string Nombre,
    string Apellido,
    DateOnly? FechaNacimiento,
    string TipoIdentificacion,
    string Identificacion);

public record struct PaqueteCarritoRequest(
    int ClienteId,
    int ServicioId,
    string IdPaqueteProveedor,
    string Nombre,
    string Ciudad,
    string Pais,
    string TipoActividad,
    int Capacidad,
    decimal PrecioNormal,
    decimal PrecioActual,
    string? ImagenUrl,
    int Duracion,
    DateTime FechaInicio,
    int Personas,
    string BookingUserId,
    IReadOnlyCollection<PaqueteTuristaRequest> Turistas);
