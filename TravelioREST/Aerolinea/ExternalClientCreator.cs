using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Aerolinea;

public class ClienteRequestData
{
    public required string nombre { get; set; }
    public required string apellido { get; set; }
    public required string correo { get; set; }
}

public class ClienteResponse
{
    public int IdUsuario { get; set; }
    public required string NombreCompleto { get; set; }
    public required string Email { get; set; }
}

public static class ExternalClientCreator
{
    public static async Task<ClienteResponse> CreateExternalClientAsync(string url, string nombre, string apellido, string correo)
    {
        var request = new ClienteRequestData
        {
            nombre = nombre,
            apellido = apellido,
            correo = correo
        };
        var response = await Global.CachedHttpClient.PostAsJsonAsync(url, request);
        var client = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        return client ?? throw new InvalidOperationException();
    }
}
