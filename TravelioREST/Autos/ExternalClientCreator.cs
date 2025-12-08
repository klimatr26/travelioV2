using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Autos;

public sealed class ClientRequest
{
    public int? BookingUserId { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public string? Telefono { get; set; }
    public string? Pais { get; set; }
}

public sealed class NuevoClienteResponse
{
    public int IdUsuario { get; set; }
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string Estado { get; set; }
    public required Link[] _links { get; set; }
}

public static class ExternalClientCreator
{
    public static async Task<NuevoClienteResponse> CrearClienteExternoAsync(string url,
        int? bookingUserId,
        string nombre,
        string apellido,
        string email,
        string? telefono,
        string? pais)
    {
        var request = new ClientRequest()
        {
            BookingUserId = bookingUserId,
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Telefono = telefono,
            Pais = pais
        };
        var response = await Global.CachedHttpClient.PostAsJsonAsync(url, request);
        var cliente = await response.Content.ReadFromJsonAsync<NuevoClienteResponse>();
        return cliente ?? throw new InvalidOperationException("No se pudo crear el cliente externo.");
    }
}
