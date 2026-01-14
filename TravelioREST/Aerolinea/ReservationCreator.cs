using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TravelioREST.Aerolinea;

// Formato original (otras aerolíneas como Withfly)
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

// Formato SkyAndes/SkaywardAir (según swagger)
public class ReservaRequestSkyAndes
{
    public int UserId { get; set; }
    public int[] FlightIds { get; set; }
    public PaymentInfo Payment { get; set; }
    public PasajeroSkyAndes[] Pasajeros { get; set; }
}

public class PaymentInfo
{
    public long CuentaOrigen { get; set; }
    public long CuentaDestino { get; set; }
}

public class PasajeroSkyAndes
{
    public string FullName { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public string Nationality { get; set; }
    public SeatInfo[] Seats { get; set; }
}

public class SeatInfo
{
    public int FlightId { get; set; }
    public int SeatId { get; set; }
    public string HoldId { get; set; }
}

// Respuesta directa de SkaywardAir/SkyAndes
public class ReservaResponseDirect
{
    public int IdReserva { get; set; }
    public string CodigoReserva { get; set; }
    public string Estado { get; set; }
    public decimal Total { get; set; }
    public object _links { get; set; }
}

// Respuesta con wrapper
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
    // Cuenta default de Travelio
    private const int CuentaTravelio = 242;

    public static async Task<ReservaResponse> CreateReservationAsync(string uri,
        string idVuelo,
        string idHold,
        string correo,
        (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[] pasajeros,
        int cuentaProveedor = 0)
    {
        // Si tenemos cuenta de proveedor, usar formato moderno con Payment
        if (cuentaProveedor > 0)
        {
            try
            {
                return await CreateReservationSkyAndesAsync(uri, idVuelo, idHold, correo, pasajeros, cuentaProveedor);
            }
            catch (HttpRequestException ex)
            {
                // Si es error de pago/saldo, re-lanzar para que el caller lo maneje
                var errorMsg = ex.Message?.ToLowerInvariant() ?? "";
                if (errorMsg.Contains("saldo") || errorMsg.Contains("payment") || errorMsg.Contains("cuenta"))
                {
                    throw;
                }
                // Si es 400 por otro motivo, intentar formato alternativo
            }
            catch (Exception ex)
            {
                // Log pero continuar con formato alternativo
                System.Diagnostics.Debug.WriteLine($"Formato moderno falló: {ex.Message}");
            }
        }

        // Intentar formato original (otras aerolíneas como Withfly)
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
        
        return await ParseReservaResponseAsync(response);
    }

    private static async Task<ReservaResponse> CreateReservationSkyAndesAsync(string uri,
        string idVuelo,
        string idHold,
        string correo,
        (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[] pasajeros,
        int cuentaProveedor = 0)
    {
        // Parsear idVuelo a entero
        int flightId = int.TryParse(idVuelo, out var fid) ? fid : 0;

        // Generar pasajeros con el formato SkyAndes
        var pasajerosSkyAndes = new List<PasajeroSkyAndes>();
        int seatId = 1;
        
        foreach (var p in pasajeros)
        {
            pasajerosSkyAndes.Add(new PasajeroSkyAndes
            {
                FullName = $"{p.nombre} {p.apellido}",
                DocumentNumber = p.identificacion,
                BirthDate = p.fechaNacimiento,
                Nationality = "Ecuador",
                Seats = new[]
                {
                    new SeatInfo
                    {
                        FlightId = flightId,
                        SeatId = seatId++,
                        HoldId = idHold
                    }
                }
            });
        }

        var request = new ReservaRequestSkyAndes
        {
            UserId = 0, // El sistema Travelio maneja usuarios internamente
            FlightIds = new[] { flightId },
            Payment = new PaymentInfo
            {
                CuentaOrigen = CuentaTravelio, // Cuenta de Travelio
                CuentaDestino = cuentaProveedor > 0 ? cuentaProveedor : 0 // Cuenta del proveedor
            },
            Pasajeros = pasajerosSkyAndes.ToArray()
        };

        var response = await Global.CachedHttpClient.PostAsJsonAsync(uri, request);
        response.EnsureSuccessStatusCode();

        return await ParseReservaResponseAsync(response);
    }

    private static async Task<ReservaResponse> ParseReservaResponseAsync(HttpResponseMessage response)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        
        // Intentar formato wrapper primero
        try
        {
            var reservaResponse = JsonSerializer.Deserialize<ReservaResponse>(jsonString);
            if (reservaResponse?.data != null && !string.IsNullOrEmpty(reservaResponse.data.idReserva))
            {
                return reservaResponse;
            }
        }
        catch { }
        
        // Intentar formato directo
        try
        {
            var directResponse = JsonSerializer.Deserialize<ReservaResponseDirect>(jsonString);
            if (directResponse != null && (directResponse.IdReserva > 0 || !string.IsNullOrEmpty(directResponse.CodigoReserva)))
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
        
        throw new InvalidOperationException($"No se pudo deserializar la respuesta de reserva: {jsonString}");
    }
}
