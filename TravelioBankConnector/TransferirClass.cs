using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace TravelioBankConnector;

internal sealed class TransaccionRequest
{
    public int cuenta_origen { get; set; }
    public int cuenta_destino { get; set; }
    public decimal monto { get; set; }
}

public static class TransferirClass
{
    public const int cuentaDefaultTravelio = 242;
    const string apiUrl = "http://mibanca.runasp.net/api/Transacciones";

    public static async Task<bool> RealizarTransferenciaAsync(int cuentaDestino, decimal monto, int cuentaOrigen = cuentaDefaultTravelio, string apiUrl = apiUrl)
    {
        var client = Bank.cachedClient;
        var request = new TransaccionRequest
        {
            cuenta_origen = cuentaOrigen,
            cuenta_destino = cuentaDestino,
            monto = monto
        };
        var response = await client.PostAsJsonAsync(apiUrl, request);
        return response.IsSuccessStatusCode;
    }
}
