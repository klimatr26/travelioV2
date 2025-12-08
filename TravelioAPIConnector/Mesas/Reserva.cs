using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Mesas;

public record struct Reserva(
    string Mensaje,
    string IdReserva,
    int IdMesa,
    DateTime Fecha,
    int NumeroPersonas,
    string Estado,
    string TipoMesa,
    string NombreCliente,
    string ApellidoCliente,
    string Correo,
    decimal ValorPagado,
    string UriFactura);
