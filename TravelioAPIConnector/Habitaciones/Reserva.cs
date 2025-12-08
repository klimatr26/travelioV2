using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Habitaciones;

public record struct Reserva(
    int IdReserva,
    decimal CostoTotal,
    DateTime FechaRegistro,
    DateTime FechaInicio,
    DateTime FechaFin,
    string EstadoGeneral,
    string Nombre,
    string Apellido,
    string Correo,
    string IdHabitacion,
    string NombreHabitacion,
    string TipoHabitacion,
    string Hotel,
    string Ciudad,
    string Pais,
    int CapacidadReserva,
    decimal CostoCalculado,
    decimal Descuento,
    decimal Impuestos,
    string IdHold,
    string Amenidades,
    string[] Imagenes,
    string UrlFacturaPdf
    );
