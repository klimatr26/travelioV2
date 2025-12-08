using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Mesas;

public record struct Mesa(
    int IdMesa,
    int IdRestaurante,
    int NumeroMesa,
    string TipoMesa,
    int Capacidad,
    decimal Precio,
    string ImagenUrl,
    string Estado);
