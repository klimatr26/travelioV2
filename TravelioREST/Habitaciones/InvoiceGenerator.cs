using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Habitaciones;

public class InvoiceRequest
{
    public int idReserva { get; set; }
    public string nombre { get; set; }
    public string apellido { get; set; }
    public string tipoDocumento { get; set; }
    public string documento { get; set; }
    public string correo { get; set; }
}

public class InvoiceResponse
{
    public int idFactura { get; set; }
    public int idReserva { get; set; }
    public decimal subtotal { get; set; }
    public decimal descuento { get; set; }
    public decimal impuesto { get; set; }
    public decimal total { get; set; }
    public string? urlPdf { get; set; }
    public _LinksInvoice _links { get; set; }
}

public class _LinksInvoice
{
    public SelfInvoiceRef self { get; set; }
    public ReservaInvoiceRef reserva { get; set; }
    public PdfInvoiceRef pdf { get; set; }
}

public class SelfInvoiceRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class ReservaInvoiceRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class PdfInvoiceRef
{
    public string href { get; set; }
    public string method { get; set; }
}


public static class InvoiceGenerator
{
    public static async Task<InvoiceResponse> GenerarFacturaAsync(
        string uri,
        int idReserva,
        string nombre,
        string apellido,
        string tipoDocumento,
        string documento,
        string correo)
    {
        var request = new InvoiceRequest
        {
            idReserva = idReserva,
            nombre = nombre,
            apellido = apellido,
            tipoDocumento = tipoDocumento,
            documento = documento,
            correo = correo
        };
        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, request);
        response.EnsureSuccessStatusCode();
        var invoiceResponse = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        return invoiceResponse ?? throw new InvalidOperationException();
    }
}