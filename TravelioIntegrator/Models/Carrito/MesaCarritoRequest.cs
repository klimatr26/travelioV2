namespace TravelioIntegrator.Models.Carrito;

public record struct MesaCarritoRequest(
    int ClienteId,
    int ServicioId,
    int IdMesa,
    int IdRestaurante,
    int NumeroMesa,
    string TipoMesa,
    int Capacidad,
    decimal Precio,
    string? ImagenUrl,
    string EstadoMesa,
    DateTime FechaReserva,
    int NumeroPersonas);
