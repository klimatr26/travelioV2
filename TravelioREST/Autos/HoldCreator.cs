using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Autos;

public sealed class HoldRequest
{
    public required string IdVehiculo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int DuracionHoldSegundos { get; set; }
}


public class HoldResponseV2
{
    public int id_hold { get; set; }
    public string mensaje { get; set; }
    public DateTime expiracion { get; set; }
}



public sealed class HoldResponse
{
    public required Datos datos { get; set; }
    public required Link[] _links { get; set; }
}

public sealed class Datos
{
    public required string IdHold { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public required Link[] Links { get; set; }
}


public static class HoldCreator
{
    public static async Task<HoldResponseV2> CrearPrerreservaAsync(string url,
        string idAuto,
        DateTime dateFrom,
        DateTime dateTo,
        int duracionHold)
    {
        var request = new HoldRequest()
        {
            IdVehiculo = idAuto,
            FechaInicio = dateFrom,
            FechaFin = dateTo,
            DuracionHoldSegundos = duracionHold
        };

        var response = await Global.CachedHttpClient.PostAsJsonAsync(url, request);
        var hold = await response.Content.ReadFromJsonAsync<HoldResponseV2>();
        return hold ?? throw new InvalidOperationException();
    }
}
