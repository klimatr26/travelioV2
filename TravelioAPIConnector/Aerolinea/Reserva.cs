using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Aerolinea;

public record struct Reserva(
    string IdReserva,
    string Origen,
    string Destino,
    string Correo,
    DateTime Fecha,
    string TipoCabina,
    (string nombre, string apellido, string tipoIdentificacion, string identificacion)[] Pasajeros,
    string NombreAerolinea,
    int AsientosReservados,
    decimal ValorPagado,
    string UriFactura,
    string Estado
    );
