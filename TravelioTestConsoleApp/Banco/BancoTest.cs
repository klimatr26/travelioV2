using System;
using System.Collections.Generic;
using System.Text;
using TravelioBankConnector;

namespace TravelioTestConsoleApp.Banco;

public static class BancoTest
{
    const int CuentaDestinoTest = 237;

    public static async Task RunTransferTest()
    {
        Console.WriteLine("Iniciando prueba de transferencia bancaria...");
        var success = false;
        try
        {
            success = await Bank.RealizarTransferenciaAsync(CuentaDestinoTest, 420.69m);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la prueba de transferencia bancaria: {ex.Message}");
        }
        Console.WriteLine(success
            ? "Prueba de transferencia bancaria exitosa."
            : "Prueba de transferencia bancaria fallida.");

        Console.WriteLine("Obteniendo saldo de cuenta para verificación...");
        try
        {
            var saldo = await Bank.ObtenerCuentasClienteAsync();
            Console.WriteLine($"Saldo actual de la cuenta de Travelio: {saldo:C}");
            saldo = await Bank.ObtenerCuentasClienteAsync(CuentaDestinoTest);
            Console.WriteLine($"Saldo actual de la cuenta de prueba: {saldo:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener el saldo de la cuenta: {ex.Message}");
        }
        Console.WriteLine("Prueba de transferencia bancaria finalizada.");
    }
}
