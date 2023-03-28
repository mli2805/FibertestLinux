using System.Security.Cryptography;
using System.Text;

namespace Fibertest.Graph
{
    public static class Password
    {
        public static string Generate(int length)
        {
            const string valid = "1234567890!@#$%&abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                byte index = GetSymbol(valid.Length);
                res.Append(valid[index]);
            }
            return res.ToString();
        }

        // This method chooses one symbol. The input parameter is the number of different allowed symbols.
        private static byte GetSymbol(int allowedSymbolsNumber)
        {
            if (allowedSymbolsNumber <= 0)
                throw new ArgumentOutOfRangeException("allowedSymbolsNumber");

            // Create a byte array to hold the random value.
            byte[] randomNumber = new byte[1];
            do
            {
                // Fill the array with a random value.
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
            }
            while (!IsFairNumber(randomNumber[0], allowedSymbolsNumber));

            return (byte)(randomNumber[0] % allowedSymbolsNumber);
        }

        private static bool IsFairNumber(byte roll, int numSides)
        {
            // There are MaxValue / numSides full sets of numbers that can come up
            // in a single byte.  For instance, if we have a 6 sided die, there are
            // 42 full sets of 1-6 that come up.  The 43rd set is incomplete.
            int fullSetsOfValues = Byte.MaxValue / numSides;

            // If the roll is within this range of fair values, then we let it continue.
            // In the 6 sided die case, a roll between 0 and 251 is allowed.  (We use
            // < rather than <= since the = portion allows through an extra 0 value).
            // 252 through 255 would provide an extra 0, 1, 2, 3 so they are not fair
            // to use.
            return roll < numSides * fullSetsOfValues;
        }
    }
}