using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using static TravelioREST.Global;

namespace TravelioREST.Aerolinea;

public static class VuelosGetter
{
    public static async Task<Vuelo[]> GetVuelosAsync(string url)
    {
        var vuelos = await CachedHttpClient.GetFromJsonAsync<Vuelo[]>(url);
        return vuelos ?? throw new InvalidOperationException();
    }
}

public sealed class Vuelo
{
    public int IdVuelo { get; set; }
    public required string Origen { get; set; }
    public required string Destino { get; set; }
    public DateTime FechaSalida { get; set; }
    public DateTime FechaLlegada { get; set; }
    public required string TipoCabina { get; set; }
    public int Pasajeros { get; set; }
    public required string NombreAerolinea { get; set; }
    public decimal PrecioNormal { get; set; }
    public decimal PrecioActual { get; set; }
    public required string Moneda { get; set; }
    public int CapacidadDisponible { get; set; }
}
