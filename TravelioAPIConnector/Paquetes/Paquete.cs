using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Paquetes;

public record struct Paquete(
    string IdPaquete,
    string Nombre,
    string Ciudad,
    string Pais,
    string TipoActividad,
    int Capacidad,
    decimal PrecioNormal,
    decimal PrecioActual,
    string ImagenUrl,
    int Duracion);
