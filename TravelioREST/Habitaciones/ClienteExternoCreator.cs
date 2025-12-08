using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Habitaciones;

public class ClienteRequest
{
    public string bookingUserId { get; set; } // Correo electrónico
    public string nombre { get; set; }
    public string apellido { get; set; }
}

public class ClienteResponse
{
    public int id { get; set; }
    public string nombre { get; set; }
    public string apellido { get; set; }
    public string documento { get; set; }
    public string correo { get; set; }
    public string tipoDocumento { get; set; }
    public bool estado { get; set; }
    public DateTime fechaModificacion { get; set; }
    public _LinksCliente _links { get; set; }
}

public class _LinksCliente
{
    public SelfClienteRef self { get; set; }
    public ReservaClienteRef reserva { get; set; }
    public CrearholdClienteRef crearHold { get; set; }
}

public class SelfClienteRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class ReservaClienteRef
{
    public string href { get; set; }
    public string method { get; set; }
    public string descripcion { get; set; }
}

public class CrearholdClienteRef
{
    public string href { get; set; }
    public string method { get; set; }
}


public static class ClienteExternoCreator
{
    public static async Task<ClienteResponse> CrearClienteExternoAsync(string uri, string correo, string nombre, string apellido)
    {
        var cliente = new ClienteRequest
        {
            bookingUserId = correo,
            nombre = nombre,
            apellido = apellido
        };
        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, cliente);
        response.EnsureSuccessStatusCode();
        var clienteResponse = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        return clienteResponse ?? throw new InvalidOperationException();
    }
}
