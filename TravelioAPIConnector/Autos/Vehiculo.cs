using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioAPIConnector.Autos;

public record struct Vehiculo(
    string IdAuto,
    string Tipo,
    int CapacidadPasajeros,
    decimal PrecioNormalPorDia,
    decimal PrecioActualPorDia,
    decimal DescuentoPorcentaje,
    string UriImagen,
    string Ciudad,
    string Pais
    );
