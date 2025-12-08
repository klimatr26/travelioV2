using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Habitaciones;

public class HoldRequest
{
    public required string idHabitacion { get; set; }
    public DateTime fechaInicio { get; set; }
    public DateTime fechaFin { get; set; }
    public int numeroHuespedes { get; set; }
    public int? duracionHoldSeg { get; set; }
    public decimal? precioActual { get; set; }
}


public class HoldResponse
{
    public required string idHold { get; set; }
    public _LinksHold _links { get; set; }
}

public class _LinksHold
{
    public SelfHoldRef self { get; set; }
    public ConfirmarHoldRef confirmar { get; set; }
    public CancelarHoldRef cancelar { get; set; }
}

public class SelfHoldRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class ConfirmarHoldRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class CancelarHoldRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public static class HoldCreator
{
    public static async Task<HoldResponse> CrearHoldAsync(
        string uri,
        string idHabitacion,
        DateTime fechaInicio,
        DateTime fechaFin,
        int numeroHuespedes,
        int? duracionHoldSegundos = null,
        decimal? precioActual = null)
    {
        var holdRequest = new HoldRequest
        {
            idHabitacion = idHabitacion,
            fechaInicio = fechaInicio,
            fechaFin = fechaFin,
            numeroHuespedes = numeroHuespedes,
            duracionHoldSeg = duracionHoldSegundos,
            precioActual = precioActual
        };
        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, holdRequest);
        response.EnsureSuccessStatusCode();
        var holdResponse = await response.Content.ReadFromJsonAsync<HoldResponse>();
        return holdResponse ?? throw new InvalidOperationException();
    }
}
