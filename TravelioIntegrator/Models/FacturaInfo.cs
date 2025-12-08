namespace TravelioIntegrator.Models;

public record struct FacturaInfo(
    string NombreFactura,
    string TipoDocumento,
    string Documento,
    string CorreoFactura);
