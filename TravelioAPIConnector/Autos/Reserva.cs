using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Autos;

public record struct Reserva(
    string NumeroMatricula,
    string Correo,
    DateTime FechaInicio,
    DateTime FechaFin,
    string Categoria,
    string Transmision,
    decimal ValorPagado,
    string UriFactura
    );
