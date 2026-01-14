using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TravelioREST.Aerolinea;

//public sealed class ReservaRequest
//{
//    public int idVuelo { get; set; }
//    public required string holdId { get; set; }
//    public required string correo { get; set; }
//    public required Pasajero[] pasajeros { get; set; }
//}

//public sealed class Pasajero
//{
//    public required string nombre { get; set; }
//    public required string apellido { get; set; }
//    public required string identificacion { get; set; }
//}


//public class ReservaResponse
//{
//    public int IdReserva { get; set; }
//    public required string CodigoReserva { get; set; }
//    public required string Estado { get; set; }
//}

// Formato original (otras aerolíneas)
public class ReservaRequest
{
    public string idVuelo { get; set; }
    public string idHold { get; set; }
    public PasajeroReservaRequest[] pasajeros { get; set; }
    public string correo { get; set; }
}

public class PasajeroReservaRequest
{
    public string nombre { get; set; }
    public string apellido { get; set; }
    public DateTime fechaNacimiento { get; set; }
    public string tipoIdentificacion { get; set; }
    public string identificacion { get; set; }
}

// Formato SkaywardAir (requiere FlightIds, Seats y HoldId por pasajero)
public class ReservaRequestSkayward
{
    public int[] FlightIds { get; set; }
    public string Correo { get; set; }
    public PasajeroReservaSkayward[] Pasajeros { get; set; }
}

public class PasajeroReservaSkayward
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string FechaNacimiento { get; set; }
    public string TipoIdentificacion { get; set; }
    public string Identificacion { get; set; }
    public string[] Seats { get; set; }
    public string HoldId { get; set; }
}

// Respuesta directa de SkaywardAir
public class ReservaResponseSkayward
{
    public int IdReserva { get; set; }
    public string CodigoReserva { get; set; }
    public string Estado { get; set; }
    public decimal Total { get; set; }
    public object _links { get; set; }
}

public class ReservaResponse
{
    public bool success { get; set; }
    public string message { get; set; }
    public DataReservaResponse data { get; set; }
    public string[] errors { get; set; }
    public DateTime timestamp { get; set; }
}

public class DataReservaResponse
{
    public bool success { get; set; }
    public string idReserva { get; set; }
    public string codigoReserva { get; set; }
    public decimal total { get; set; }
    public string mensaje { get; set; }
}

public static class ReservationCreator
{
    public static async Task<ReservaResponse> CreateReservationAsync(string uri,
        string idVuelo,
        string idHold,
        string correo,
        (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[] pasajeros)
    {
        // Primero intentar formato SkaywardAir (FlightIds + Seats/HoldId por pasajero)
        try
        {
            return await CreateReservationSkaywardAsync(uri, idVuelo, idHold, correo, pasajeros);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Si falla con 400, intentar formato original
        }
        catch
        {
            // Si falla por otra razón, intentar formato original
        }

        // Intentar formato original (otras aerolíneas)
        var reservaRequest = new ReservaRequest
        {
            idVuelo = idVuelo,
            idHold = idHold,
            correo = correo,
            pasajeros = pasajeros.Select(p => new PasajeroReservaRequest
            {
                nombre = p.nombre,
                apellido = p.apellido,
                tipoIdentificacion = p.tipoIdentificacion,
                identificacion = p.identificacion,
                fechaNacimiento = p.fechaNacimiento
            }).ToArray()
        };

        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, reservaRequest);
        response.EnsureSuccessStatusCode();
        
        var jsonString = await response.Content.ReadAsStringAsync();
        
        // Intentar formato wrapper primero
        try
        {
            var reservaResponse = JsonSerializer.Deserialize<ReservaResponse>(jsonString);
            if (reservaResponse?.data != null)
            {
                return reservaResponse;
            }
        }
        catch { }
        
        // Intentar formato directo
        try
        {
            var directResponse = JsonSerializer.Deserialize<ReservaResponseSkayward>(jsonString);
            if (directResponse != null && directResponse.IdReserva > 0)
            {
                return new ReservaResponse
                {
                    success = true,
                    data = new DataReservaResponse
                    {
                        success = true,
                        idReserva = directResponse.IdReserva.ToString(),
                        codigoReserva = directResponse.CodigoReserva ?? directResponse.IdReserva.ToString(),
                        total = directResponse.Total
                    }
                };
            }
        }
        catch { }
        
        throw new InvalidOperationException("No se pudo deserializar la respuesta de reserva");
    }

    private static async Task<ReservaResponse> CreateReservationSkaywardAsync(string uri,
        string idVuelo,
        string idHold,
        string correo,
        (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[] pasajeros)
    {
        // Generar asientos automáticamente para cada pasajero
        var pasajerosSkayward = new List<PasajeroReservaSkayward>();
        for (int i = 0; i < pasajeros.Length; i++)
        {
            int row = (i / 6) + 1;
            char col = (char)('A' + (i % 6));
            string seat = $"{row}{col}";

            pasajerosSkayward.Add(new PasajeroReservaSkayward
            {
                Nombre = pasajeros[i].nombre,
                Apellido = pasajeros[i].apellido,
                TipoIdentificacion = pasajeros[i].tipoIdentificacion,
                Identificacion = pasajeros[i].identificacion,
                FechaNacimiento = pasajeros[i].fechaNacimiento.ToString("yyyy-MM-dd"),
                Seats = new[] { seat },
                HoldId = idHold
            });
        }

        // Parsear idVuelo a entero
        int flightId = int.TryParse(idVuelo, out var fid) ? fid : 0;

        var request = new ReservaRequestSkayward
        {
            FlightIds = new[] { flightId },
            Correo = correo,
            Pasajeros = pasajerosSkayward.ToArray()
        };

        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, request);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();

        // Intentar formato directo de SkaywardAir
        try
        {
            var directResponse = JsonSerializer.Deserialize<ReservaResponseSkayward>(jsonString);
            if (directResponse != null && directResponse.IdReserva > 0)
            {
                return new ReservaResponse
                {
                    success = true,
                    data = new DataReservaResponse
                    {
                        success = true,
                        idReserva = directResponse.IdReserva.ToString(),
                        codigoReserva = directResponse.CodigoReserva ?? directResponse.IdReserva.ToString(),
                        total = directResponse.Total
                    }
                };
            }
        }
        catch { }

        // Intentar formato wrapper
        var reservaResponse = JsonSerializer.Deserialize<ReservaResponse>(jsonString);
        return reservaResponse ?? throw new InvalidOperationException("No se pudo deserializar la respuesta de reserva");
    }
}
