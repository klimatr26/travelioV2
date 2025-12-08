using System;
using TravelioAPIConnector;
using TravelioAPIConnector.Aerolinea;

namespace TravelioTestConsoleApp.Aerolinea;

internal static class ConnectionTest
{
    public static async Task TestSoapConnectionAsync()
    {
        if (Global.IsREST)
        {
            throw new NotSupportedException("La version REST de aerolinea debe reimplementarse.");
        }

        const string soapIntegracionUri = @"<REEMPLAZAR_SOAP_INTEGRACION>";

        var vuelos = await Connector.GetVuelosAsync(soapIntegracionUri);
        if (vuelos.Length == 0)
        {
            Console.WriteLine("No se encontraron vuelos.");
            return;
        }

        var vuelo = vuelos[0];
        Console.WriteLine($"Vuelo seleccionado: {vuelo}");

        const int pasajeros = 2;
        var disponible = await Connector.VerificarDisponibilidadVueloAsync(soapIntegracionUri, vuelo.IdVuelo, pasajeros);
        Console.WriteLine($"El vuelo {vuelo.IdVuelo} {(disponible ? "esta" : "no esta")} disponible para {pasajeros} pasajeros.");

        var pasajerosInfo = new (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[]
        {
            ("Ana", "Gomez", "DNI", "12345678", new DateTime(1990, 1, 1)),
            ("Luis", "Lopez", "DNI", "87654321", new DateTime(1992, 5, 10))
        };

        var (holdId, expira) = await Connector.CrearPrerreservaVueloAsync(
            soapIntegracionUri,
            vuelo.IdVuelo,
            pasajerosInfo,
            duracionHold: 300);
        Console.WriteLine($"Hold creado: {holdId}, expira {expira}.");

        var (idReserva, codigoReserva, mensaje) = await Connector.CrearReservaAsync(
            soapIntegracionUri,
            vuelo.IdVuelo,
            holdId,
            "correo@correo.com",
            pasajerosInfo);
        Console.WriteLine($"Reserva creada: Id {idReserva}, Codigo {codigoReserva}, Mensaje: {mensaje}");

        var facturaUrl = await Connector.GenerarFacturaAsync(
            soapIntegracionUri,
            idReserva,
            1000m,
            120m,
            1120m,
            ("Carlos Lopez", "DNI", "12345678", "correo@correo.com"));
        Console.WriteLine($"Factura generada: {facturaUrl}");

        var datos = await Connector.GetDatosReservaAsync(soapIntegracionUri, idReserva);
        Console.WriteLine($"Datos de la reserva: {datos}");
    }
}
