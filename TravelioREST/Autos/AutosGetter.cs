using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Autos;

public sealed class AutosResponse
{
    public required Datum[] Data { get; set; }
    public required Link[] Links { get; set; }
}

public sealed class Datum
{
    public required string IdAuto { get; set; }
    public required string Tipo { get; set; }
    public int Capacidad { get; set; }
    public decimal PrecioNormal { get; set; }
    public decimal? PrecioActual { get; set; }
    public required string UriImagen { get; set; }
    public required string Ciudad { get; set; }
    public required string Pais { get; set; }
}

public static class AutosGetter
{
    public static async Task<AutosResponse> GetAutosAsync(string uri)
    {
        var result = await Global.CachedHttpClient.GetFromJsonAsync<AutosResponse>(uri);
        return result ?? throw new InvalidOperationException();
    }
}
