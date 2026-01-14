using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using static TravelioREST.Global;

namespace TravelioREST.Aerolinea;

// Modelo para la respuesta real del API (SkaywardAir)
public class ConsultaVuelosResponse
{
    public int count { get; set; }
    public FlightConsulta[] flights { get; set; }
    public _LinksConsultaVuelos _links { get; set; }
}

public class _LinksConsultaVuelos
{
    public string self { get; set; }
    public string hold { get; set; }
    public string availability { get; set; }
    public string book { get; set; }
}

// Converter para manejar IdVuelo como string o número
public class StringOrIntConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt64().ToString();
        }
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public class FlightConsulta
{
    [JsonConverter(typeof(StringOrIntConverter))]
    public string IdVuelo { get; set; }
    public string Origen { get; set; }
    public string Destino { get; set; }
    public DateTime FechaSalida { get; set; }
    public DateTime FechaLlegada { get; set; }
    public string TipoCabina { get; set; }
    public int Pasajeros { get; set; }
    public string NombreAerolinea { get; set; }
    public decimal PrecioNormal { get; set; }
    public decimal PrecioActual { get; set; }
    public string Moneda { get; set; }
    public int CapacidadDisponible { get; set; }
}


public static class VuelosGetter
{
    public static async Task<FlightConsulta[]> GetVuelosAsync(string baseUri,
        string? from = null,
        string? to = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? cabin = null,
        int? pasajeros = null,
        decimal? precio_min = null,
        decimal? precio_max = null)
    {
        var uriBuilder = new UriBuilder(baseUri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        if (!string.IsNullOrEmpty(from))
            query["from"] = Uri.EscapeDataString(from);

        if (!string.IsNullOrEmpty(to))
            query["to"] = Uri.EscapeDataString(to);

        if (dateFrom.HasValue)
            query["date_from"] = dateFrom.ToString();

        if (dateTo.HasValue)
            query["date_to"] = dateTo.ToString();

        if (!string.IsNullOrEmpty(cabin))
            query["cabin"] = Uri.EscapeDataString(cabin);

        if (pasajeros.HasValue)
            query["pasajeros"] = pasajeros.ToString();

        if (precio_min.HasValue)
            query["precio_min"] = precio_min.ToString();

        if (precio_max.HasValue)
            query["precio_max"] = precio_max.ToString();

        uriBuilder.Query = query.ToString();

        var url = uriBuilder.ToString();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var vuelos = await CachedHttpClient.GetFromJsonAsync<ConsultaVuelosResponse>(url, options);
        return vuelos?.flights ?? throw new InvalidOperationException();
    }
}