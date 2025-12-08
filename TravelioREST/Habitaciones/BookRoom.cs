using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioREST.Habitaciones;

public class BookRoomRequest
{
    public string idHabitacion { get; set; }
    public string idHold { get; set; }
    public string nombre { get; set; }
    public string apellido { get; set; }
    public string correo { get; set; }
    public string tipoDocumento { get; set; }
    public string documento { get; set; }
    public DateTime fechaInicio { get; set; }
    public DateTime fechaFin { get; set; }
    public int numeroHuespedes { get; set; }
}

public class BookRoomResponse
{
    public int idReserva { get; set; }
    public decimal costoTotalReserva { get; set; }
    public DateTime fechaRegistro { get; set; }
    public DateTime fechaInicio { get; set; }
    public DateTime fechaFin { get; set; }
    public string estadoGeneral { get; set; }
    public bool estado { get; set; }
    public string nombre { get; set; }
    public string apellido { get; set; }
    public string correo { get; set; }
    public string tipoDocumento { get; set; }
    public string habitacion { get; set; }
    public decimal precioNormal { get; set; }
    public decimal precioActual { get; set; }
    public int capacidad { get; set; }
    public _LinksBookRoom _links { get; set; }
}

public class _LinksBookRoom
{
    public SelfBookRoomRef self { get; set; }
    public DisponibilidadBookRoomRef disponibilidad { get; set; }
    public FacturaBookRoomRef factura { get; set; }
    public CancelarBookRoomRef cancelar { get; set; }
}

public class SelfBookRoomRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class DisponibilidadBookRoomRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class FacturaBookRoomRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public class CancelarBookRoomRef
{
    public string href { get; set; }
    public string method { get; set; }
}

public static class BookRoom
{
    public static async Task<BookRoomResponse> ReservarHabitacionAsync(
        string uri,
        string idHabitacion,
        string idHold,
        string nombre,
        string apellido,
        string correo,
        string tipoDocumento,
        string documento,
        DateTime fechaInicio,
        DateTime fechaFin,
        int numeroHuespedes)
    {
        var reservaRequest = new BookRoomRequest
        {
            idHabitacion = idHabitacion,
            idHold = idHold,
            nombre = nombre,
            apellido = apellido,
            correo = correo,
            tipoDocumento = tipoDocumento,
            documento = documento,
            fechaInicio = fechaInicio,
            fechaFin = fechaFin,
            numeroHuespedes = numeroHuespedes
        };
        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, reservaRequest);
        response.EnsureSuccessStatusCode();
        var reservaResponse = await response.Content.ReadFromJsonAsync<BookRoomResponse>();
        return reservaResponse ?? throw new InvalidOperationException();
    }
}
