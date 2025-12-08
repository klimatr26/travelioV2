using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioREST.Aerolinea;

public static class ReservationDataGetter
{
    public static async Task<string> GetBookingDataContent(string uri)
    {
        var vuelos = await Global.CachedHttpClient.GetStringAsync(uri);
        return vuelos;
    }
}
