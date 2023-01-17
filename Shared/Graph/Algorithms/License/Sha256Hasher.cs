using System.Security.Cryptography;
using System.Text;

namespace Fibertest.Graph
{
    public static class Sha256Hasher
    {
        public static string? GetHashString(this string? inputString)
        {
            if (inputString == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString(@"X2"));

            return sb.ToString();
        }

        private static byte[] GetHash(this string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
    }
}
