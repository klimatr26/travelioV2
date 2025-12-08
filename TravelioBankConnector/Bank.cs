namespace TravelioBankConnector;

public static class Bank
{
    internal static readonly HttpClient cachedClient = new();

    public static Task<bool> RealizarTransferenciaAsync(int cuentaDestino, decimal monto)
    {
        return TransferirClass.RealizarTransferenciaAsync(cuentaDestino, monto);
    }

    public static Task<decimal> ObtenerCuentasClienteAsync(int numeroCuenta = InfoCuentas.CuentaOrigenDefault)
    {
        return InfoCuentas.ObtenerCuentasClienteAsync(numeroCuenta);
    }
}
