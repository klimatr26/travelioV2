using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using static TravelioREST.Global;

namespace TravelioREST.Aerolinea;

public sealed class DisponibilidadRequest
{
    public int idVuelo { get; set; }
    public int pasajeros { get; set; }
}

public class Disponibilidad
{
    public bool disponible { get; set; }
}


public static class VueloCheckAvailable
{
    public static async Task<bool> GetDisponibilidadAsync(string url, int idVuelo, int numPasajeros)
    {
        var request = new DisponibilidadRequest
        {
            idVuelo = idVuelo,
            pasajeros = numPasajeros
        };
        var response = await CachedHttpClient.PostAsJsonAsync(url, request);
        var disponibilidad = await response.Content.ReadFromJsonAsync<Disponibilidad>();
        return disponibilidad?.disponible ?? false;
    }
}
