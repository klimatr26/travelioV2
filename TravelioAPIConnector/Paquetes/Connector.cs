using System;
using System.ServiceModel;
using TravelioSOAP.Paquetes;
using static TravelioAPIConnector.Global;

namespace TravelioAPIConnector.Paquetes;

#pragma warning disable CS0162
public static class Connector
{
    public static async Task<Paquete[]> BuscarPaquetesAsync(
        string uri,
        string? ciudad = null,
        DateTime? fechaInicio = null,
        string? tipoActividad = null,
        decimal? precioMax = null)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para paquetes aun no esta disponible.");
        }

        var soapClient = new PaquetesServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.BuscarPaquetesAsync(ciudad, fechaInicio?.ToString("yyyy-MM-dd"), tipoActividad, precioMax);
        var paquetes = response?.BuscarPaquetesResult ?? [];

        return Array.ConvertAll(paquetes, dto => new Paquete(
            dto.IdPaquete ?? string.Empty,
            dto.Nombre ?? string.Empty,
            dto.Ciudad ?? string.Empty,
            dto.Pais ?? string.Empty,
            dto.TipoActividad ?? string.Empty,
            dto.Capacidad ?? 0,
            dto.PrecioNormal,
            dto.PrecioActual,
            dto.ImagenUrl ?? string.Empty,
            dto.Duracion ?? 0));
    }

    public static async Task<bool> ValidarDisponibilidadAsync(string uri, string idPaquete, DateTime fechaInicio, int personas)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para paquetes aun no esta disponible.");
        }

        var soapClient = new PaquetesServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        return await soapClient.ValidarDisponibilidadAsync(idPaquete, fechaInicio.ToString("yyyy-MM-dd"), personas);
    }

    public static async Task<(string holdId, DateTime expira)> CrearHoldAsync(
        string uri,
        string idPaquete,
        string bookingUserId,
        DateTime fechaInicio,
        int personas,
        int duracionSegundos = 300)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para paquetes aun no esta disponible.");
        }

        var soapClient = new PaquetesServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.CrearHoldAsync(idPaquete, bookingUserId, fechaInicio.ToString("yyyy-MM-dd"), personas, duracionSegundos);
        return (response.HoldId, response.Expira);
    }

    public static async Task<string> CrearUsuarioExternoAsync(string uri, string bookingUserId, string nombre, string apellido, string correo)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para paquetes aun no esta disponible.");
        }

        var soapClient = new PaquetesServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.CrearUsuarioExternoAsync(bookingUserId, nombre, apellido, correo);
        return response?.IdUsuario ?? throw new InvalidOperationException("No se pudo crear el usuario externo.");
    }

    public static async Task<Reserva> CrearReservaAsync(
        string uri,
        string idPaquete,
        string holdId,
        string bookingUserId,
        string metodoPago,
        (string nombre, string apellido, DateTime? fechaNacimiento, string tipoIdentificacion, string identificacion)[] turistas)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para paquetes aun no esta disponible.");
        }

        var soapClient = new PaquetesServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));

        var turistasSoap = Array.ConvertAll(turistas, t => new TuristaSoap
        {
            Nombre = t.nombre,
            Apellido = t.apellido,
            FechaNacimiento = t.fechaNacimiento,
            TipoIdentificacion = t.tipoIdentificacion,
            Identificacion = t.identificacion
        });

        var reserva = await soapClient.ReservarPaqueteAsync(idPaquete, holdId, bookingUserId, metodoPago, turistasSoap);

        return reserva is null
            ? throw new InvalidOperationException("No se pudo crear la reserva del paquete.")
            : new Reserva(
                reserva.IdReserva ?? string.Empty,
                reserva.CodigoReserva ?? string.Empty,
                reserva.ClienteId ?? 0,
                reserva.UsuarioId ?? 0,
                reserva.Total,
                reserva.FechaCreacion,
                reserva.Estado ?? string.Empty);
    }

    public static async Task<string> EmitirFacturaAsync(string uri, string reservaId, decimal subtotal, decimal iva, decimal total)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para paquetes aun no esta disponible.");
        }

        var soapClient = new PaquetesServiceSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var factura = await soapClient.EmitirFacturaAsync(reservaId, subtotal, iva, total);
        return factura?.UriFactura ?? throw new InvalidOperationException("No se pudo emitir la factura del paquete.");
    }
}
#pragma warning restore CS0162
