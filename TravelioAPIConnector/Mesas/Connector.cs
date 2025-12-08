using System;
using System.ServiceModel;
using TravelioSOAP.Mesas.Busqueda;
using TravelioSOAP.Mesas.Disponibilidad;
using TravelioSOAP.Mesas.Factura;
using TravelioSOAP.Mesas.Reserva;
using TravelioSOAP.Mesas.Usuario;
using static TravelioAPIConnector.Global;

namespace TravelioAPIConnector.Mesas;

#pragma warning disable CS0162
public static class Connector
{
    public static async Task<Mesa[]> BuscarMesasAsync(string uri, int? capacidad = null, string? tipoMesa = null, string? estado = null)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para mesas aun no esta disponible.");
        }

        var soapClient = new BusBusquedaWSSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.BuscarMesasAsync(capacidad, tipoMesa, estado);
        var mesas = response?.BuscarMesasResponse?.BuscarMesasResult?.Mesas ?? [];

        return Array.ConvertAll(mesas, static m => new Mesa(
            m.IdMesa,
            m.IdRestaurante,
            m.NumeroMesa,
            m.TipoMesa ?? string.Empty,
            m.Capacidad,
            m.Precio,
            m.ImagenURL ?? string.Empty,
            m.Estado ?? string.Empty));
    }

    public static async Task<bool> ValidarDisponibilidadAsync(string uri, int idMesa, DateTime fecha, int numeroPersonas)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para mesas aun no esta disponible.");
        }

        var soapClient = new BusDisponibilidadWSSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.ValidarDisponibilidadMesaAsync(idMesa, fecha.ToString(), numeroPersonas);
        return response?.Body.ValidarDisponibilidadMesaResponse?.ValidarDisponibilidadResult?.Disponible ?? false;
    }

    public static async Task<(string holdId, DateTime expira)> CrearPreReservaAsync(
        string uri,
        int idMesa,
        DateTime fecha,
        int personas,
        int duracionHoldSegundos = 300)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para mesas aun no esta disponible.");
        }

        var soapClient = new BusReservaWSSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.CrearPreReservaAsync(idMesa.ToString(), fecha.ToString(), personas, duracionHoldSegundos);
        var pre = response?.CrearPreReservaResult ?? throw new InvalidOperationException("No se pudo crear la prerreserva.");
        return (pre.IdHold ?? string.Empty, DateTime.TryParse(pre.FechaReserva, out var parsed) ? parsed : DateTime.MinValue);
    }

    public static async Task<int> CrearUsuarioAsync(string uri, string nombre, string apellido, string email, string tipoIdentificacion, string identificacion)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para mesas aun no esta disponible.");
        }

        var soapClient = new BusUsuarioWSSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.CrearUsuarioAsync(nombre, apellido, email, tipoIdentificacion, identificacion);
        return response?.CrearUsuarioResult?.IdUsuario ?? throw new InvalidOperationException("No se pudo crear el usuario.");
    }

    public static async Task<Reserva> ConfirmarReservaAsync(
        string uri,
        int idMesa,
        string holdId,
        string nombre,
        string apellido,
        string correo,
        string tipoIdentificacion,
        string identificacion,
        DateTime fecha,
        int personas)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para mesas aun no esta disponible.");
        }

        var soapClient = new BusReservaWSSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.ConfirmarReservaAsync(
            idMesa.ToString(),
            holdId,
            nombre,
            apellido,
            correo,
            tipoIdentificacion,
            identificacion,
            fecha.ToString(),
            personas);

        var reserva = response?.ConfirmarReservaResult ?? throw new InvalidOperationException("No se pudo confirmar la reserva.");

        return new Reserva(
            reserva.Mensaje ?? string.Empty,
            reserva.IdReserva ?? string.Empty,
            reserva.IdMesa,
            DateTime.TryParse(reserva.FechaReserva, out var fechaReserva) ? fechaReserva : DateTime.MinValue,
            reserva.NumeroPersonas,
            string.Empty,
            string.Empty,
            reserva.NombreCliente ?? string.Empty,
            reserva.ApellidoCliente ?? string.Empty,
            reserva.Correo ?? string.Empty,
            reserva.ValorPagado,
            reserva.UriFactura ?? string.Empty);
    }

    public static async Task<Reserva> BuscarReservaAsync(string uri, int idReserva)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para mesas aun no esta disponible.");
        }

        var soapClient = new BusReservaWSSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var response = await soapClient.BuscarReservaAsync(idReserva.ToString());
        var datos = response?.BuscarReservaResult ?? throw new InvalidOperationException("No se pudieron obtener los datos de la reserva.");

        return new Reserva(
            datos.Mensaje ?? string.Empty,
            datos.IdReserva.ToString(),
            datos.IdMesa,
            datos.Fecha,
            datos.NumeroPersonas,
            datos.Estado ?? string.Empty,
            datos.TipoMesa ?? string.Empty,
            datos.NombreCliente ?? string.Empty,
            datos.ApellidoCliente ?? string.Empty,
            datos.Correo ?? string.Empty,
            datos.ValorPagado,
            datos.UriFactura ?? string.Empty);
    }

    public static async Task<string> GenerarFacturaAsync(
        string uri,
        string idReserva,
        string correo,
        string nombre,
        string tipoIdentificacion,
        string identificacion,
        decimal valor)
    {
        if (IsREST)
        {
            throw new NotImplementedException("La integracion REST para mesas aun no esta disponible.");
        }

        var soapClient = new BusFacturaWSSoapClient(GetBinding(uri), new EndpointAddress(uri));
        var factura = await soapClient.GenerarFacturaBusAsync(idReserva, correo, nombre, tipoIdentificacion, identificacion, valor);
        return factura?.uri_factura ?? throw new InvalidOperationException("No se pudo generar la factura.");
    }
}
#pragma warning restore CS0162
