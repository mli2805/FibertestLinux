using System.Security.Cryptography;
using System.Text;

namespace Fibertest.Utils
{
    public static class Sha256Ext
    {
        public static string GetSha256(this string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            var hashComputer = SHA256.Create();
            byte[] hash = hashComputer.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += $"{x:x2}";
            }
            return hashString;
        }
    }
}
