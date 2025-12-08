using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Habitaciones;

public record struct Habitacion(
    string IdHabitacion,
    string NombreHabitacion,
    string TipoHabitacion,
    string Hotel,
    string Ciudad,
    string Pais,
    int Capacidad,
    decimal PrecioNormal,
    decimal PrecioActual,
    decimal PrecioVigente,
    string Amenidades,
    string[] Imagenes
    );
