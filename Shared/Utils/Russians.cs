namespace Fibertest.Utils
{
    public static class Russians
    {
        public static string GetYearInRussian(this int number)
        {
            if (number % 10 == 1 && number != 11)
            {
                return "год";
            }

            if ((number % 10 == 2 || number % 10 == 3 || number % 10 == 4) && (number < 12 || number > 14))
            {
                return "года";
            }

            return "лет";
        }

    }
}
