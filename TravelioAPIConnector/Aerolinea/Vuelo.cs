using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Aerolinea;

public record struct Vuelo(
    string IdVuelo,
    string Origen,
    string Destino,
    DateTime Fecha,
    string TipoCabina,
    string NombreAerolinea,
    int CapacidadPasajeros,
    int CapacidadActual,
    decimal PrecioNormal,
    decimal PrecioActual,
    decimal DescuentoPorcentaje
    );
    
