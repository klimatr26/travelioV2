using System;
using TravelioAPIConnector;
using TravelioAPIConnector.Habitaciones;

namespace TravelioTestConsoleApp.Habitaciones;

internal static class ConnectionTest
{
    public static async Task TestBasicConnectionAsync()
    {
        Console.WriteLine($"Probando {(Global.IsREST ? "REST" : "SOAP")}");

        const string restBuscarHabitacionesUri = @"http://aureacuenrest.runasp.net/api/v1/hoteles/search";
        const string soapBuscarHabitacionesUri = @"http://aureacuen.runasp.net/buscarHabitacionesWS.asmx";
        var buscarHabitacionesUri = Global.IsREST ? restBuscarHabitacionesUri : soapBuscarHabitacionesUri;

        const string restValidarDisponibilidadUri = @"http://aureacuenrest.runasp.net/api/v1/hoteles/availability";
        const string soapValidarDisponibilidadUri = @"http://aureacuen.runasp.net/ValidarDisponibilidadWS.asmx";
        var validarDisponibilidadUri = Global.IsREST ? restValidarDisponibilidadUri : soapValidarDisponibilidadUri;

        const string restCrearPreReservaUri = @"http://aureacuenrest.runasp.net/api/v1/hoteles/hold";
        const string soapCrearPreReservaUri = @"http://aureacuen.runasp.net/CrearPreReservaWS.asmx";
        var crearPreReservaUri = Global.IsREST ? restCrearPreReservaUri : soapCrearPreReservaUri;

        const string restCrearUsuarioExternoUri = @"http://aureacuenrest.runasp.net/api/v1/hoteles/usuarios/externo";
        const string soapCrearUsuarioExternoUri = @"http://aureacuen.runasp.net/CrearUsuarioExternoWS.asmx";
        var crearUsuarioExternoUri = Global.IsREST ? restCrearUsuarioExternoUri : soapCrearUsuarioExternoUri;

        const string restReservarHabitacionUri = @"http://aureacuenrest.runasp.net/api/v1/hoteles/book";
        const string soapReservarHabitacionUri = @"http://aureacuen.runasp.net/ReservarHabitacionWS.asmx";
        var reservarHabitacionUri = Global.IsREST ? restReservarHabitacionUri : soapReservarHabitacionUri;

        const string restEmitirFacturaUri = @"http://aureacuenrest.runasp.net/api/v1/hoteles/invoices";
        const string soapEmitirFacturaUri = @"http://aureacuen.runasp.net/EmitirFacturaHotelWS.asmx";
        var emitirFacturaUri = Global.IsREST ? restEmitirFacturaUri : soapEmitirFacturaUri;

        const string restBuscarDatosReservaUri = @"http://aureacuenrest.runasp.net/api/v1/hoteles/reserva";
        const string soapBuscarDatosReservaUri = @"http://aureacuen.runasp.net/buscarDatosReservaWS.asmx";
        var buscarDatosReservaUri = Global.IsREST ? restBuscarDatosReservaUri : soapBuscarDatosReservaUri;

        var habitaciones = await Connector.BuscarHabitacionesAsync(buscarHabitacionesUri);
        if (habitaciones.Length == 0)
        {
            Console.WriteLine("No se encontraron habitaciones.");
            return;
        }

        var habitacion = habitaciones[^2];
        Console.WriteLine($"Habitación seleccionada: {habitacion}");

        var fechaInicio = DateTime.Now.Date.AddMonths(3);
        var fechaFin = fechaInicio.AddDays(3);

        var disponible = await Connector.ValidarDisponibilidadAsync(validarDisponibilidadUri, habitacion.IdHabitacion, fechaInicio, fechaFin);
        Console.WriteLine($"La habitacion {habitacion.IdHabitacion} {(disponible ? "está" : "no está")} disponible entre {fechaInicio:d} y {fechaFin:d}.");

        if (!disponible)
            return;

        var holdId = await Connector.CrearPrerreservaAsync(
            crearPreReservaUri,
            habitacion.IdHabitacion,
            fechaInicio,
            fechaFin,
            numeroHuespedes: habitacion.Capacidad,
            precioActual: habitacion.PrecioActual);
        Console.WriteLine($"Hold creado: {holdId}");

        var usuarioId = await Connector.CrearUsuarioExternoAsync(crearUsuarioExternoUri, "j12perez@correo.com", "Juan", "Pérez");
        Console.WriteLine($"Usuario externo creado: {usuarioId}");

        var reservaId = await Connector.CrearReservaAsync(
            reservarHabitacionUri,
            habitacion.IdHabitacion,
            holdId,
            "Juan",
            "Pérez",
            "juan@correo.com",
            "DNI",
            "1234567890",
            fechaInicio,
            fechaFin,
            habitacion.Capacidad);
        Console.WriteLine($"Reserva creada con Id: {reservaId}");

        var facturaUrl = await Connector.EmitirFacturaAsync(
            emitirFacturaUri,
            reservaId,
            "Juan",
            "Pérez",
            "DNI",
            "1234567890",
            "juan@correo.com");
        Console.WriteLine($"Factura emitida: {facturaUrl}");

        var datosReserva = await Connector.ObtenerDatosReservaAsync(buscarDatosReservaUri, reservaId);
        Console.WriteLine($"Datos de la reserva: {datosReserva}");
    }
}
