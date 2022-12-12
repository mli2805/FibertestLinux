namespace Fibertest.Graph
{
    public static class VersionExt
    {
        public static bool IsOlder(this string version, string version2)
        {
            var parts = version.GetNumericParts();
            var parts2 = version2.GetNumericParts();
            if (parts == null || parts2 == null) return false;

            for (int i = 0; i < 4; i++)
            {
                if (parts[i] < parts2[i]) return true;
                if (parts[i] > parts2[i]) return false;
            }

            return false;
        }

        private static int[]? GetNumericParts(this string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            var parts = version.Split('.');
            if (parts.Length != 4) return null;

            var result = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(parts[i], out int number))
                    return null;
                result[i] = number;
            }

            return result;
        }
    }
}
