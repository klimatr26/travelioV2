namespace TravelioIntegrator.Models.Carrito;

public record struct HabitacionCarritoRequest(
    int ClienteId,
    int ServicioId,
    string IdHabitacionProveedor,
    string NombreHabitacion,
    string TipoHabitacion,
    string Hotel,
    string Ciudad,
    string Pais,
    int Capacidad,
    decimal PrecioNormal,
    decimal PrecioActual,
    decimal PrecioVigente,
    string Amenidades,
    string? Imagenes,
    DateTime FechaInicio,
    DateTime FechaFin,
    int NumeroHuespedes);
