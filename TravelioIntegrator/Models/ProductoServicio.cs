namespace TravelioIntegrator.Models;

public record struct ProductoServicio<TProducto>(int ServicioId, string ServicioNombre, TProducto Producto);
