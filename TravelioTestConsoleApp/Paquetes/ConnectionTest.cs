using System;
using TravelioAPIConnector;
using TravelioAPIConnector.Paquetes;

namespace TravelioTestConsoleApp.Paquetes;

internal static class ConnectionTest
{
    public static async Task TestSoapConnectionAsync()
    {
        if (Global.IsREST)
        {
            throw new NotSupportedException("La version REST de paquetes aun no esta disponible.");
        }

        const string soapBuscarPaquetesUri = @"<REEMPLAZAR_SOAP_BUSCAR_PAQUETES>";
        const string soapValidarDisponibilidadUri = @"<REEMPLAZAR_SOAP_VALIDAR_DISPONIBILIDAD>";
        const string soapCrearHoldUri = @"<REEMPLAZAR_SOAP_CREAR_HOLD>";
        const string soapCrearUsuarioExternoUri = @"<REEMPLAZAR_SOAP_CREAR_USUARIO_EXTERNO>";
        const string soapReservarPaqueteUri = @"<REEMPLAZAR_SOAP_RESERVAR_PAQUETE>";
        const string soapEmitirFacturaUri = @"<REEMPLAZAR_SOAP_EMITIR_FACTURA>";

        var paquetes = await Connector.BuscarPaquetesAsync(soapBuscarPaquetesUri);
        if (paquetes.Length == 0)
        {
            Console.WriteLine("No se encontraron paquetes.");
            return;
        }

        var paquete = paquetes[0];
        Console.WriteLine($"Paquete seleccionado: {paquete}");

        var fechaInicio = DateTime.Now.Date.AddMonths(1);
        const int personas = 2;

        var disponible = await Connector.ValidarDisponibilidadAsync(soapValidarDisponibilidadUri, paquete.IdPaquete, fechaInicio, personas);
        Console.WriteLine($"El paquete {paquete.IdPaquete} {(disponible ? "esta" : "no esta")} disponible desde {fechaInicio:d} para {personas} personas.");

        const string bookingUserId = "usuario@correo.com";

        var (holdId, expira) = await Connector.CrearHoldAsync(
            soapCrearHoldUri,
            paquete.IdPaquete,
            bookingUserId,
            fechaInicio,
            personas,
            duracionSegundos: 300);
        Console.WriteLine($"Hold creado: {holdId}, expira: {expira}.");

        var usuarioId = await Connector.CrearUsuarioExternoAsync(soapCrearUsuarioExternoUri, bookingUserId, "Juan", "Perez", bookingUserId);
        Console.WriteLine($"Usuario externo creado: {usuarioId}");

        var turistas = new (string nombre, string apellido, DateTime? fechaNacimiento, string tipoIdentificacion, string identificacion)[]
        {
            ("Ana", "Gomez", new DateTime(1990, 1, 1), "DNI", "12345678"),
            ("Luis", "Lopez", new DateTime(1992, 5, 10), "DNI", "87654321")
        };

        var reserva = await Connector.CrearReservaAsync(
            soapReservarPaqueteUri,
            paquete.IdPaquete,
            holdId,
            bookingUserId,
            "Tarjeta",
            turistas);

        Console.WriteLine($"Reserva creada: Id {reserva.IdReserva}, Codigo {reserva.CodigoReserva}, Total {reserva.Total}");

        var facturaUrl = await Connector.EmitirFacturaAsync(soapEmitirFacturaUri, reserva.IdReserva, 1000m, 120m, 1120m);
        Console.WriteLine($"Factura generada: {facturaUrl}");
    }
}
