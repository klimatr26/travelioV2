using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Paquetes;

public record struct Reserva(
    string IdReserva,
    string CodigoReserva,
    int ClienteId,
    int UsuarioId,
    decimal Total,
    DateTime FechaCreacion,
    string Estado);
