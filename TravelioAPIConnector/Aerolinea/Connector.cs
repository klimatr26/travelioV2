using System;
using System.ServiceModel;
using TravelioSOAP.Aerolinea;
using static TravelioAPIConnector.Global;

namespace TravelioAPIConnector.Aerolinea;

#pragma warning disable CS0162
public static class Connector
{
    public static async Task<Vuelo[]> GetVuelosAsync(
        string uri,
        string? origen = null,
        string? destino = null,
        DateTime? fechaDespegue = null,
        DateTime? fechaLlegada = null,
        string? tipoCabina = null,
        int? pasajeros = null,
        decimal? precioMin = null,
        decimal? precioMax = null)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST de aerolinea debe reimplementarse.");
        }

        var client = new IntegracionServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await client.buscarVuelosAsync(origen, destino, fechaDespegue, fechaLlegada, tipoCabina, pasajeros, precioMin, precioMax);
        var vuelos = response?.buscarVuelosResult ?? [];

        return Array.ConvertAll(vuelos, static v => new Vuelo(
            v.IdVuelo ?? string.Empty,
            v.Origen ?? string.Empty,
            v.Destino ?? string.Empty,
            v.Fecha,
            v.TipoCabina ?? string.Empty,
            v.NombreAerolinea ?? string.Empty,
            v.CapacidadPasajeros,
            v.CapacidadActual,
            v.PrecioNormal,
            v.PrecioActual,
            v.PrecioNormal == 0 ? 0 : (1 - (v.PrecioActual / v.PrecioNormal)) * 100m));
    }

    public static async Task<bool> VerificarDisponibilidadVueloAsync(string uri, string idVuelo, int pasajeros)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST de aerolinea debe reimplementarse.");
        }

        var client = new IntegracionServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        return await client.validarDisponibilidadVueloAsync(idVuelo, pasajeros);
    }

    public static async Task<(string holdId, DateTime expira)> CrearPrerreservaVueloAsync(
        string uri,
        string idVuelo,
        (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[] pasajeros,
        int duracionHold = 300)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST de aerolinea debe reimplementarse.");
        }

        var client = new IntegracionServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var pasajerosDto = Array.ConvertAll(pasajeros, p => new PasajeroDTO_Integracion
        {
            Nombre = p.nombre,
            Apellido = p.apellido,
            TipoIdentificacion = p.tipoIdentificacion,
            Identificacion = p.identificacion,
            FechaNacimiento = p.fechaNacimiento
        });

        var response = await client.crearPreReservaVueloDetalleAsync(idVuelo, pasajerosDto, duracionHold);
        var pre = response ?? throw new InvalidOperationException("No se pudo crear la prerreserva.");
        return (pre.IdHold ?? string.Empty, pre.ExpiresAt);
    }

    public static async Task<(string idReserva, string codigoReserva, string mensaje)> CrearReservaAsync(
        string uri,
        string idVuelo,
        string idHold,
        string correo,
        (string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento)[] pasajeros)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST de aerolinea debe reimplementarse.");
        }

        var client = new IntegracionServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));

        var pasajerosDto = Array.ConvertAll(pasajeros, static p => new PasajeroDTO_Integracion
        {
            Nombre = p.nombre,
            Apellido = p.apellido,
            TipoIdentificacion = p.tipoIdentificacion,
            Identificacion = p.identificacion,
            FechaNacimiento = p.fechaNacimiento
        });

        var response = await client.reservarVueloDetalleAsync(idVuelo, idHold, pasajerosDto, correo);
        var reserva = response ?? throw new InvalidOperationException("No se pudo crear la reserva.");
        return (reserva.IdReserva ?? string.Empty, reserva.CodigoReserva ?? string.Empty, reserva.Message ?? string.Empty);
    }

    public static async Task<string> GenerarFacturaAsync(
        string uri,
        string idReserva,
        decimal subtotal,
        decimal iva,
        decimal total,
        (string nombre, string tipoDocumento, string documento, string correo) cliente,
        string idTransaccionBanco = "")
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST de aerolinea debe reimplementarse.");
        }

        var client = new IntegracionServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var dtoCliente = new ClienteFacturaDTO
        {
            Nombre = cliente.nombre,
            TipoDocumento = cliente.tipoDocumento,
            Documento = cliente.documento,
            Correo = cliente.correo
        };

        var response = await client.emitirFacturaVueloDetalleAsync(idReserva, subtotal, iva, total, dtoCliente, idTransaccionBanco);
        return response?.UriFactura ?? throw new InvalidOperationException("No se pudo emitir la factura.");
    }

    public static async Task<string> CrearClienteExternoAsync(
        string uri,
        string correo,
        string nombre,
        string apellido,
        DateTime fechaNacimiento,
        string tipoIdentificacion,
        string identificacion)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST de aerolinea debe reimplementarse.");
        }

        var client = new IntegracionServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await client.crearUsuarioExternoDetalleAsync(correo, nombre, apellido, fechaNacimiento, tipoIdentificacion, identificacion);
        return response?.IdUsuario ?? throw new InvalidOperationException("No se pudo crear el usuario externo.");
    }

    public static async Task<Reserva> GetDatosReservaAsync(string uri, string idReserva)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST de aerolinea debe reimplementarse.");
        }

        var client = new IntegracionServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var datos = await client.buscarDatosReservaAsync(idReserva) ?? throw new InvalidOperationException("No se pudo obtener la reserva.");

        var pasajeros = datos.Pasajeros ?? Array.Empty<PasajeroDTO_Integracion>();

        return new Reserva(
            datos.IdReserva ?? string.Empty,
            datos.Origen ?? string.Empty,
            datos.Destino ?? string.Empty,
            datos.Correo ?? string.Empty,
            datos.Fecha,
            datos.TipoCabina ?? string.Empty,
            Array.ConvertAll(pasajeros, p => (p.Nombre, p.Apellido, p.TipoIdentificacion, p.Identificacion)),
            datos.NombreAerolinea ?? string.Empty,
            datos.AsientosReservados,
            datos.ValorPagado,
            datos.UriFactura ?? string.Empty,
            datos.Estado ?? string.Empty);
    }
}
#pragma warning restore CS0162
