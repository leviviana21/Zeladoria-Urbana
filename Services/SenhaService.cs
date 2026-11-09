using System.Security.Cryptography;
using System.Text;

namespace ZeladoriaUrbana.API.Services;
public static class SenhaService
{
    private const int Iterations = 210_000;
    public static string GerarHash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
    public static bool Verificar(string senha, string? salvo)
    {
        if (string.IsNullOrEmpty(salvo)) return false;
        var partes = salvo.Split('$');
        if (partes.Length == 4 && partes[0] == "PBKDF2" && int.TryParse(partes[1], out var iter) && iter >= 100_000 && iter <= 2_000_000)
        {
            try
            {
                var salt = Convert.FromBase64String(partes[2]);
                var esperado = Convert.FromBase64String(partes[3]);
                var recebido = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iter, HashAlgorithmName.SHA256, esperado.Length);
                return CryptographicOperations.FixedTimeEquals(recebido, esperado);
            }
            catch (FormatException) { return false; }
        }
        // Compatibilidade temporária: hashes SHA-256 importados do banco original.
        if (salvo.Length == 64 && salvo.All(Uri.IsHexDigit))
        {
            var legado = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(senha)));
            return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(salvo), Convert.FromHexString(legado));
        }
        return false;
    }
    public static bool Legado(string hash) => hash.Length == 64 && hash.All(Uri.IsHexDigit);
}
