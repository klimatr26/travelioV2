namespace TravelioIntegrator.Models;

public record struct FiltroVuelos(
    string? Origen = null,
    string? Destino = null,
    DateTime? FechaDespegue = null,
    DateTime? FechaLlegada = null,
    string? TipoCabina = null,
    int? Pasajeros = null,
    decimal? PrecioMin = null,
    decimal? PrecioMax = null);

public record struct FiltroAutos(
    string? Categoria = null,
    string? Transmision = null,
    int? Capacidad = null,
    decimal? PrecioMin = null,
    decimal? PrecioMax = null,
    string? Sort = null,
    string? Ciudad = null,
    string? Pais = null);

public record struct FiltroHabitaciones(
    DateTime? FechaInicio = null,
    DateTime? FechaFin = null,
    string? TipoHabitacion = null,
    int? Capacidad = null,
    decimal? PrecioMin = null,
    decimal? PrecioMax = null);

public record struct FiltroPaquetes(
    string? Ciudad = null,
    DateTime? FechaInicio = null,
    string? TipoActividad = null,
    decimal? PrecioMax = null);

public record struct FiltroMesas(
    int? Capacidad = null,
    string? TipoMesa = null,
    string? Estado = null);
