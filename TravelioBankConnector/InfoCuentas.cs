using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioBankConnector;

public sealed class CuentaDetallesResponse
{
    public object? Clientes { get; set; }
    public object[] Servicios { get; set; }
    public object[] Transacciones { get; set; }
    public object[] Transacciones1 { get; set; }
    public object? TiposCuentas { get; set; }
    public int cuenta_id { get; set; }
    public required string cliente_id { get; set; }
    public required string tipo_cuenta { get; set; }
    public decimal saldo { get; set; }
    public DateTime? fecha_creacion { get; set; }
}

public static class InfoCuentas
{
    public const int CuentaOrigenDefault = 242;
    public const string ApiUrlCuentas = "http://mibanca.runasp.net/api/Cuentas";

    public static async Task<decimal> ObtenerCuentasClienteAsync(int numeroCuenta = CuentaOrigenDefault, string uri = ApiUrlCuentas)
    {
        var client = Bank.cachedClient;
        var response = await client.GetFromJsonAsync<CuentaDetallesResponse>(uri.EndsWith('/') ? $"{uri}{numeroCuenta}" : $"{uri}/{numeroCuenta}");
        return response?.saldo ?? throw new InvalidOperationException();
    }
}
