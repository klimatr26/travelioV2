using System;
using TravelioAPIConnector;
using TravelioAPIConnector.Mesas;

namespace TravelioTestConsoleApp.Mesas;

internal static class ConnectionTest
{
    public static async Task TestSoapConnectionAsync()
    {
        if (Global.IsREST)
        {
            throw new NotSupportedException("La version REST de mesas aun no esta disponible.");
        }

        const string soapBuscarMesasUri = @"<REEMPLAZAR_SOAP_BUSCAR_MESAS>";
        const string soapValidarDisponibilidadUri = @"<REEMPLAZAR_SOAP_VALIDAR_DISPONIBILIDAD>";
        const string soapCrearPreReservaUri = @"<REEMPLAZAR_SOAP_CREAR_PRERESERVA>";
        const string soapCrearUsuarioUri = @"<REEMPLAZAR_SOAP_CREAR_USUARIO>";
        const string soapConfirmarReservaUri = @"<REEMPLAZAR_SOAP_CONFIRMAR_RESERVA>";
        const string soapGenerarFacturaUri = @"<REEMPLAZAR_SOAP_GENERAR_FACTURA>";
        const string soapBuscarReservaUri = @"<REEMPLAZAR_SOAP_BUSCAR_RESERVA>";

        var mesas = await Connector.BuscarMesasAsync(soapBuscarMesasUri);
        if (mesas.Length == 0)
        {
            Console.WriteLine("No se encontraron mesas.");
            return;
        }

        var mesa = mesas[0];
        Console.WriteLine($"Mesa seleccionada: {mesa}");

        var fecha = DateTime.Now.Date.AddDays(7);
        const int personas = 2;

        var disponible = await Connector.ValidarDisponibilidadAsync(soapValidarDisponibilidadUri, mesa.IdMesa, fecha, personas);
        Console.WriteLine($"La mesa {mesa.IdMesa} {(disponible ? "esta" : "no esta")} disponible el {fecha:d}.");

        var (holdId, expira) = await Connector.CrearPreReservaAsync(
            soapCrearPreReservaUri,
            mesa.IdMesa,
            fecha,
            personas,
            duracionHoldSegundos: 300);
        Console.WriteLine($"Hold creado: {holdId}, expira: {expira}.");

        var usuarioId = await Connector.CrearUsuarioAsync(soapCrearUsuarioUri, "Juan", "Perez", "correo@correo.com", "DNI", "12345678");
        Console.WriteLine($"Usuario creado: {usuarioId}");

        var reserva = await Connector.ConfirmarReservaAsync(
            soapConfirmarReservaUri,
            mesa.IdMesa,
            holdId,
            "Juan",
            "Perez",
            "correo@correo.com",
            "DNI",
            "12345678",
            fecha,
            personas);
        Console.WriteLine($"Reserva creada: Id {reserva.IdReserva}, Valor pagado {reserva.ValorPagado}");

        var facturaUrl = await Connector.GenerarFacturaAsync(
            soapGenerarFacturaUri,
            reserva.IdReserva,
            "correo@correo.com",
            "Juan Perez",
            "DNI",
            "12345678",
            valor: 100m);
        Console.WriteLine($"Factura generada: {facturaUrl}");

        var reservaBuscada = await Connector.BuscarReservaAsync(soapBuscarReservaUri, int.Parse(reserva.IdReserva));
        Console.WriteLine($"Reserva consultada: {reservaBuscada}");
    }
}
