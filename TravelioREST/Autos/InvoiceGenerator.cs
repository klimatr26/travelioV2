using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Autos;

public class InvoiceRequest
{
    public required string id_reserva { get; set; }
    public required string correo { get; set; }
    public required string nombre { get; set; }
    public required string tipo_identificacion { get; set; }
    public required string identificacion { get; set; }
    public decimal valor { get; set; }
}


public class InvoiceResponse
{
    public required DatosInvoiceResponse datos { get; set; }
    public required Link[] links { get; set; }
}

public class DatosInvoiceResponse
{
    public required string id_factura { get; set; }
    public required string url_factura { get; set; }
    public required string estado { get; set; }
}


public static class InvoiceGenerator
{
    public static async Task<string> GenerarFacturaAsync(string url, int reservaId, decimal subtotal, decimal iva, decimal total, string nombre, string tipoDocumento, string documento, string correo)
    {
        var request = new InvoiceRequest()
        {
            id_reserva = reservaId.ToString(),
            correo = correo,
            nombre = nombre,
            tipo_identificacion = tipoDocumento,
            identificacion = documento,
            valor = total
        };

        var response = await Global.CachedHttpClient.PostAsJsonAsync(url, request);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        return invoice?.datos.url_factura ?? throw new InvalidOperationException();
    }
}
