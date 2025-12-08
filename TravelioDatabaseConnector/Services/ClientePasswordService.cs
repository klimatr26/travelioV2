using TravelioDatabaseConnector.Models;
using TravelioDatabaseConnector.Security;

namespace TravelioDatabaseConnector.Services;

public static class ClientePasswordService
{
    public static void EstablecerPassword(Cliente cliente, string passwordPlano)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        var (hash, salt) = PasswordHasher.CreateHashWithSalt(passwordPlano);
        cliente.PasswordHash = hash;
        cliente.PasswordSalt = salt;
    }

    public static bool EsPasswordValido(Cliente cliente, string passwordPlano)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        return PasswordHasher.VerifyPassword(passwordPlano, cliente.PasswordHash, cliente.PasswordSalt);
    }
}
