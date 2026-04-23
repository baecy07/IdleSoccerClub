using System;

namespace IdleSoccerClubMVP.Core.Economy
{
    public static class NumberNotationFormatter
    {
        private static readonly string[] CompactSuffixes =
        {
            string.Empty,
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi",
            "Sx",
            "Sp",
            "Oc",
            "No",
            "Dc"
        };

        public static string FormatWhole(long value)
        {
            return value.ToString("N0");
        }

        public static string FormatForUi(long value)
        {
            return Math.Abs(value) >= 1000000L
                ? FormatCompact(value)
                : FormatWhole(value);
        }

        public static string FormatCompact(long value, int decimalPlaces = 1)
        {
            if (value == 0L)
            {
                return "0";
            }

            bool isNegative = value < 0L;
            double absoluteValue = Math.Abs((double)value);
            int suffixIndex = 0;

            while (absoluteValue >= 1000d && suffixIndex < CompactSuffixes.Length - 1)
            {
                absoluteValue /= 1000d;
                suffixIndex++;
            }

            string pattern = decimalPlaces <= 0 ? "0" : "0." + new string('#', decimalPlaces);
            string formatted = absoluteValue.ToString(pattern) + CompactSuffixes[suffixIndex];
            return isNegative ? "-" + formatted : formatted;
        }
    }
}
