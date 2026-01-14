using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;

namespace TravelioREST.Mesas;

public class Mesa
{
    [JsonPropertyName("idMesa")]
    public int idMesa { get; set; }
    
    [JsonPropertyName("idRestaurante")]
    public int idRestaurante { get; set; }
    
    [JsonPropertyName("numeroMesa")]
    public int numeroMesa { get; set; }
    
    [JsonPropertyName("tipoMesa")]
    public string tipoMesa { get; set; }
    
    [JsonPropertyName("capacidad")]
    public int capacidad { get; set; }
    
    [JsonPropertyName("precio")]
    public decimal precio { get; set; }
    
    [JsonPropertyName("estado")]
    public string estado { get; set; }
    
    [JsonPropertyName("imagenURL")]
    public string imagenURL { get; set; }
    
    public object? priceRange { get; set; }
}

// Clase para mapear la respuesta de la API
public class MesasListResponse
{
    [JsonPropertyName("mensaje")]
    public string? Mensaje { get; set; }
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("mesas")]
    public Mesa[]? Mesas { get; set; }
}

// http://cangrejitosfelices.runasp.net/api/v1/integracion/restaurantes/search?capacidad=1&tipoMesa=Exterior&estado=Disponible

public static class MesasList
{
    public static async Task<Mesa[]> GetMesasListAsync(string baseUri,
        int? capacidad = null,
        string? tipoMesa = null,
        string? estado = null)
    {
        var uriBuilder = new UriBuilder(baseUri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        if (capacidad.HasValue)
            query["capacidad"] = capacidad.ToString();

        if (!string.IsNullOrEmpty(tipoMesa))
            query["tipoMesa"] = Uri.EscapeDataString(tipoMesa);

        if (!string.IsNullOrEmpty(estado))
            query["estado"] = Uri.EscapeDataString(estado);

        uriBuilder.Query = query.ToString();

        var url = uriBuilder.ToString();
        var httpClient = Global.CachedHttpClient;
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var content = await response.Content.ReadAsStringAsync();
        
        // Intentar primero como array directo (nuevo formato de algunas APIs)
        try
        {
            var directArray = System.Text.Json.JsonSerializer.Deserialize<Mesa[]>(content, jsonOptions);
            if (directArray != null && directArray.Length > 0)
                return directArray;
        }
        catch { /* No es un array directo, intentar formato envuelto */ }
        
        // Intentar como objeto envuelto { mensaje, total, mesas: [...] }
        try
        {
            var mesasListResponse = System.Text.Json.JsonSerializer.Deserialize<MesasListResponse>(content, jsonOptions);
            if (mesasListResponse?.Mesas != null)
                return mesasListResponse.Mesas;
        }
        catch { /* Formato no reconocido */ }
        
        return [];
    }
}
