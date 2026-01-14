using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using static TravelioREST.Global;

namespace TravelioREST.Aerolinea;

//public class DisponibilidadRequest
//{
//    public int IdVuelo { get; set; }
//    public int Pasajeros { get; set; }
//}

//public class DisponibilidadResponse
//{
//    public bool disponible { get; set; }
//    public _LinksDisponibilidad _links { get; set; }
//}

//public class _LinksDisponibilidad
//{
//    public string self { get; set; }
//    public string hold { get; set; }
//}

public class DisponibilidadRequest
{
    public string idVuelo { get; set; }
    public int pasajeros { get; set; }
}

// Formato con wrapper "data" (algunos proveedores)
public class DisponibilidadResponseWrapped
{
    public bool success { get; set; }
    public string message { get; set; }
    public DataDisponibilidadResponse data { get; set; }
    public string[] errors { get; set; }
    public DateTime timestamp { get; set; }
}

public class DataDisponibilidadResponse
{
    public bool disponible { get; set; }
    public string mensaje { get; set; }
    public int asientosDisponibles { get; set; }
}

// Formato directo (SkaywardAir y otros)
public class DisponibilidadResponseDirect
{
    public bool disponible { get; set; }
    public object _links { get; set; }
}

public static class VueloCheckAvailable
{
    public static async Task<bool> GetDisponibilidadAsync(string url, string idVuelo, int numPasajeros)
    {
        try
        {
            var request = new DisponibilidadRequest
            {
                idVuelo = idVuelo,
                pasajeros = numPasajeros
            };
            
            var response = await CachedHttpClient.PostAsJsonAsync(url, request);
            
            // Si el servidor devuelve error, asumir disponible para no bloquear
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Disponibilidad API error {response.StatusCode} - asumiendo disponible");
                return true;
            }
            
            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // Intentar formato con wrapper "data" primero (más moderno)
            try
            {
                var wrappedResponse = System.Text.Json.JsonSerializer.Deserialize<DisponibilidadResponseWrapped>(jsonString, options);
                if (wrappedResponse?.data != null)
                {
                    return wrappedResponse.data.disponible;
                }
                // Si success es true pero no hay data, asumir disponible
                if (wrappedResponse?.success == true)
                {
                    return true;
                }
            }
            catch { }
            
            // Intentar formato directo
            try
            {
                var directResponse = System.Text.Json.JsonSerializer.Deserialize<DisponibilidadResponseDirect>(jsonString, options);
                if (directResponse != null)
                {
                    return directResponse.disponible;
                }
            }
            catch { }
            
            // Si no se pudo parsear, asumir disponible
            System.Diagnostics.Debug.WriteLine($"No se pudo parsear respuesta de disponibilidad: {jsonString}");
            return true;
        }
        catch (Exception ex)
        {
            // Si hay cualquier error, asumir disponible para no bloquear al usuario
            System.Diagnostics.Debug.WriteLine($"Error verificando disponibilidad: {ex.Message}");
            return true;
        }
    }
}
