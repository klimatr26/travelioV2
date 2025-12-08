using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Autos;


public sealed class AutoDisponibilidadRequest
{
    public required string IdVehiculo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}


public sealed class DisponibilidadResponse
{
    public int IdVehiculo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Disponible { get; set; }
    public required string Mensaje { get; set; }
    public required Link[] _links { get; set; }
}


public static class VehicleCheckAvailable
{
    public static async Task<bool> GetDisponibilidadAsync(string url, string idAuto, DateTime dateFrom, DateTime dateTo)
    {
        var request = new AutoDisponibilidadRequest
        {
            IdVehiculo = idAuto,
            FechaInicio = dateFrom,
            FechaFin = dateTo
        };

        try {
            var response = await Global.CachedHttpClient.PostAsJsonAsync(url, request);
            var disponibilidad = await response.Content.ReadFromJsonAsync<DisponibilidadResponse>();
            return disponibilidad is null
                ? throw new InvalidOperationException("No se pudo obtener la disponibilidad del vehículo.")
                : disponibilidad.Disponible;
        }
        catch
        {
            return false;
        }
    }
}
