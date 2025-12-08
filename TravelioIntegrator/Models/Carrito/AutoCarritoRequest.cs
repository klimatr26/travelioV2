namespace TravelioIntegrator.Models.Carrito;

public record struct AutoCarritoRequest(
    int ClienteId,
    int ServicioId,
    string IdAutoProveedor,
    string Tipo,
    string? Categoria,
    string? Transmision,
    int CapacidadPasajeros,
    decimal PrecioNormalPorDia,
    decimal PrecioActualPorDia,
    decimal DescuentoPorcentaje,
    string? UriImagen,
    string? Ciudad,
    string? Pais,
    DateTime FechaInicio,
    DateTime FechaFin);
