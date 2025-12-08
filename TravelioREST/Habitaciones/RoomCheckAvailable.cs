using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using TravelioREST.Aerolinea;

namespace TravelioREST.Habitaciones;

public class RoomCheckAvailableRequest
{
    public required string idHabitacion { get; set; }
    public DateTime fechaInicio { get; set; }
    public DateTime fechaFin { get; set; }
}

public class RoomCheckAvailableResponse
{
    public bool disponible { get; set; }
    public string mensaje { get; set; }
    public _LinksRoomCheckAvailable _links { get; set; }
}

public class _LinksRoomCheckAvailable
{
    public SelfRoomCheckAvailableRef self { get; set; }
    public ReservarRoomCheckAvailableRef reservar { get; set; }
    public PrereservaRoomCheckAvailableRef prereserva { get; set; }
}

public class SelfRoomCheckAvailableRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class ReservarRoomCheckAvailableRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class PrereservaRoomCheckAvailableRef
{
    public string href { get; set; }
    public string method { get; set; }
}


public static class RoomCheckAvailable
{
    public static async Task<RoomCheckAvailableResponse> CheckAvailabilityAsync(
        string uri,
        string idHabitacion,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var request = new RoomCheckAvailableRequest
        {
            idHabitacion = idHabitacion,
            fechaInicio = fechaInicio,
            fechaFin = fechaFin
        };

        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoomCheckAvailableResponse>() ?? throw new Exception("La respuesta de la API es nula.");
    }
}
