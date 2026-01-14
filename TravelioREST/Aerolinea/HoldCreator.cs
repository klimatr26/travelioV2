using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using static TravelioREST.Global;

namespace TravelioREST.Aerolinea;

//public sealed class HoldRequest
//{
//    public int idVuelo { get; set; }
//    public required string[] seats { get; set; }
//    public int duracionHoldSegundos { get; set; }
//}


//public sealed class HoldResponse
//{
//    public int idVuelo { get; set; }
//    public required string holdId { get; set; }
//    public required string expiraEn { get; set; }
//}

// Formato simple (usado por algunas aerolíneas)
public class HoldRequestSimple
{
    public string idVuelo { get; set; }
    public int duracionHoldSegundos { get; set; }
}

// Formato con pasajeros (usado por aerolíneas modernas)
public class HoldRequest
{
    public string idVuelo { get; set; }
    public string? seat { get; set; }  // Campo opcional
    public PasajeroHoldRequest[]? pasajeros { get; set; }
    public int duracionHoldSegundos { get; set; }
}

public class PasajeroHoldRequest
{
    public string nombre { get; set; }
    public string apellido { get; set; }
    public DateTime fechaNacimiento { get; set; }
    public string tipoIdentificacion { get; set; }
    public string identificacion { get; set; }
}

public class HoldResponse
{
    public bool success { get; set; }
    public string message { get; set; }
    public DataHoldResponse data { get; set; }
    public string[] errors { get; set; }
    public DateTime timestamp { get; set; }
}

// Formato directo de SkaywardAir
public class HoldResponseDirect
{
    public int IdVuelo { get; set; }
    public string HoldId { get; set; }
    public DateTime ExpiraEn { get; set; }
    public object _links { get; set; }
}

public class DataHoldResponse
{
    public bool success { get; set; }
    public string idHold { get; set; }
    public DateTime expiresAt { get; set; }
    public string mensaje { get; set; }
}

public static class HoldCreator
{
    public static async Task<HoldResponse> CreateHoldAsync(
        string uri,
        string idVuelo,
        (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[] pasajeros,
        int duracionHold = 300)
    {
        HttpResponseMessage response;
        
        // Intentar primero con formato completo (incluye pasajeros)
        try
        {
            // Generar asientos automáticamente basados en la cantidad de pasajeros
            var seats = new List<string>();
            for (int i = 0; i < pasajeros.Length; i++)
            {
                int row = (i / 6) + 1;
                char col = (char)('A' + (i % 6));
                seats.Add($"{row}{col}");
            }

            var requestCompleto = new HoldRequest
            {
                idVuelo = idVuelo,
                seat = string.Join(",", seats),
                duracionHoldSegundos = duracionHold,
                pasajeros = pasajeros.Select(p => new PasajeroHoldRequest
                {
                    nombre = p.nombre,
                    apellido = p.apellido,
                    tipoIdentificacion = p.tipoIdentificacion,
                    identificacion = p.identificacion,
                    fechaNacimiento = p.fechaNacimiento
                }).ToArray()
            };

            response = await CachedHttpClient.PostAsJsonAsync(uri, requestCompleto);
            
            if (response.IsSuccessStatusCode)
            {
                return await ParseHoldResponse(response);
            }
        }
        catch
        {
            // Si falla, intentar formato simple
        }

        // Intentar formato simple (solo idVuelo y duración)
        var requestSimple = new HoldRequestSimple
        {
            idVuelo = idVuelo,
            duracionHoldSegundos = duracionHold
        };

        response = await CachedHttpClient.PostAsJsonAsync(uri, requestSimple);
        response.EnsureSuccessStatusCode();
        
        return await ParseHoldResponse(response);
    }

    private static async Task<HoldResponse> ParseHoldResponse(HttpResponseMessage response)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        
        // Intentar formato directo de SkaywardAir primero
        try
        {
            var directResponse = System.Text.Json.JsonSerializer.Deserialize<HoldResponseDirect>(jsonString);
            if (directResponse != null && !string.IsNullOrEmpty(directResponse.HoldId))
            {
                return new HoldResponse
                {
                    success = true,
                    data = new DataHoldResponse
                    {
                        success = true,
                        idHold = directResponse.HoldId,
                        expiresAt = directResponse.ExpiraEn
                    }
                };
            }
        }
        catch { }
        
        // Intentar formato con wrapper
        var holdResponse = System.Text.Json.JsonSerializer.Deserialize<HoldResponse>(jsonString);
        return holdResponse ?? throw new InvalidOperationException("No se pudo deserializar la respuesta del hold");
    }
}
