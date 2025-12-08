using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Web;

namespace TravelioREST.Habitaciones;

public class BookedRoomInfoResponse
{
    public int idReserva { get; set; }
    public decimal costoTotal { get; set; }
    public DateTime fechaRegistro { get; set; }
    public DateTime inicio { get; set; }
    public DateTime fin { get; set; }
    public string estadoGeneral { get; set; }
    public string nombre { get; set; }
    public string apellido { get; set; }
    public string correo { get; set; }
    public string idHabitacion { get; set; }
    public string nombreHabitacion { get; set; }
    public string tipoHabitacion { get; set; }
    public string hotel { get; set; }
    public string ciudad { get; set; }
    public string pais { get; set; }
    public int capacidadReserva { get; set; }
    public decimal costoCalculado { get; set; }
    public int descuento { get; set; }
    public decimal impuestos { get; set; }
    public string idHold { get; set; }
    public string amenidades { get; set; }
    public string imagenes { get; set; }
    public string urlPdf { get; set; }
    public _LinksBookedRoomInfo _links { get; set; }
}

public class _LinksBookedRoomInfo
{
    public SelfBookedRoomInfoRef self { get; set; }
    public FacturaBookedRoomInfoRef factura { get; set; }
    public DisponibilidadBookedRoomInfoRef disponibilidad { get; set; }
    public HabitacionBookedRoomInfoRef habitacion { get; set; }
    public PdfBookedRoomInfoRef pdf { get; set; }
}

public class SelfBookedRoomInfoRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class FacturaBookedRoomInfoRef
{
    public string href { get; set; }
    public string method { get; set; }
    public BodyBookedRoomInfoRef body { get; set; }
}

public class BodyBookedRoomInfoRef
{
    public int idReserva { get; set; }
}

public class DisponibilidadBookedRoomInfoRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class HabitacionBookedRoomInfoRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class PdfBookedRoomInfoRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public static class BookedRoomInfo
{
    public static async Task<BookedRoomInfoResponse> GetBookedRoomInfoAsync(
        string uri,
        int idReserva)
    {
        var uriBuilder = new UriBuilder(uri);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["idReserva"] = idReserva.ToString();

        uriBuilder.Query = query.ToString();

        var response = await Global.CachedHttpClient.GetFromJsonAsync<BookedRoomInfoResponse>(uriBuilder.ToString());
        return response ?? throw new InvalidOperationException();
    }
}
