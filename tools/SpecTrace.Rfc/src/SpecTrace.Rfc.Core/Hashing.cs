using System.Security.Cryptography;
using System.Text;

namespace SpecTrace.Rfc.Core;

public static class Hashing
{
    public static string Sha256Text(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }
}
