using System;
using System.Collections.Generic;
using System.Text;

#nullable disable
// La API del banco está mal diseñada y no funciona correctamente, así que no tocamos este archivo por ahora.
namespace TravelioBankConnector;

public class Rootobject
{
    public Class1[] Property1 { get; set; }
}

public class Class1
{
    public object Clientes { get; set; }
    public object[] Servicios { get; set; }
    public object[] Transacciones { get; set; }
    public object[] Transacciones1 { get; set; }
    public object TiposCuentas { get; set; }
    public int cuenta_id { get; set; }
    public string cliente_id { get; set; }
    public string tipo_cuenta { get; set; }
    public float saldo { get; set; }
    public DateTime fecha_creacion { get; set; }
}



internal class CuentasClienteResponse
{
}
