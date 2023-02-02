using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
#pragma warning disable SYSLIB0011
#pragma warning disable SYSLIB0022

namespace Fibertest.Graph
{
    public static class Cryptography
    {
        private static byte[] _key = { 0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0xc5, 0x51, 0xf6, 0x4e, 0x62, 0x3f, 0x27, 0x00, 0xca };
        private static byte[] _initVector = { 0xf3, 0x5e, 0x7a, 0x81, 0xae, 0xf2, 0xe7, 0xc1, 0x8d, 0x54, 0x00, 0x8c, 0xb4, 0x92, 0xd0, 0xd8 };

        public static object Decode(byte[] bytes)
        {
            using MemoryStream fStream = new MemoryStream(bytes);
            var rmCrypto = new RijndaelManaged();

            using var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateDecryptor(_key, _initVector), CryptoStreamMode.Read);
            var binaryFormatter = new BinaryFormatter();
            return binaryFormatter.Deserialize(cryptoStream);
        }

        public static byte[]? Encode(object? obj)
        {
            if (obj == null) return null;
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                var rmCrypto = new RijndaelManaged();

                using (var cryptoStream = new CryptoStream(memoryStream, 
                           rmCrypto.CreateEncryptor(_key, _initVector), CryptoStreamMode.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(cryptoStream, obj);
                }

                return memoryStream.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}