using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Halak
{
    public static partial class JsonHelper
    {
        private const int BigExponent = 1000;
        private static NumberStyles StandardNumberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;
        private static readonly uint[] UInt32Powers10 = new[]
        {
            1U, 10U, 100U, 1000U, 10000U, 100000U, 1000000U, 10000000U, 100000000U, 1000000000U,
        };
        private static readonly ulong[] UInt64Powers10 = new[]
        {
            1UL,
            10UL,
            100UL,
            1000UL,
            10000UL,
            100000UL,
            1000000UL,
            10000000UL,
            100000000UL,
            1000000000UL,
            10000000000UL,
            100000000000UL,
            1000000000000UL,
            10000000000000UL,
            100000000000000UL,
            1000000000000000UL,
            10000000000000000UL,
            100000000000000000UL,
            1000000000000000000UL,
            10000000000000000000UL,
        };
        private static readonly double[] DoublePowers10 = new[]
        {
            1E+0, 1E+1, 1E+2, 1E+3, 1E+4, 1E+5, 1E+6, 1E+7, 1E+8, 1E+9, 1E+10, 1E+11, 1E+12, 1E+13, 1E+14, 1E+15, 1E+16, 1E+17, 1E+18, 1E+19, 1E+20, 1E+21, 1E+22, 1E+23
        };

        private static int SkipIntegerPart(string s, int end, int index)
        {
            for (; index < end; index++)
            {
                var c = s[index];
                if (c == '.' || c == 'e' || c == 'E')
                    return index;
            }

            return end;
        }

        private static int SkipFractionalPart(string s, int end, int index)
        {
            for (; index < end; index++)
            {
                var c = s[index];
                if (c == 'e' || c == 'E')
                    return index;
            }

            return end;
        }

        private static int ReadExponentIfSmall(string s, int end, ref int index)
        {
            if (end == index)
                return BigExponent;

            var sign = true;
            var c = s[index++];
            if ((c == '-' || c == '+') && index < end)
            {
                sign = (c != '-');
                c = s[index++];
            }
            if (IsDigit(c) == false)
                return BigExponent;

            var exponent = (int)ToDigit(c);
            while (index < end)
            {
                c = s[index++];
                if (IsDigit(c))
                {
                    exponent = (exponent * 10) + ToDigit(c);
                    if (exponent > BigExponent)
                        break;
                }
                else if (IsTerminal(c))
                    break;
                else
                {
                    exponent = BigExponent;
                    break;
                }
            }

            return sign ? exponent : -exponent;
        }

        public static decimal ParseDecimal(string s, int startIndex, int length, decimal defaultValue = default(decimal))
        {
            if (startIndex != 0 || s.Length != length)
                s = s.Substring(startIndex, length);

            var value = decimal.Zero;
            if (decimal.TryParse(s, StandardNumberStyles, CultureInfo.InvariantCulture, out value))
                return value;
            else
                return defaultValue;
        }

        public static decimal? ParseNullableDecimal(string s, int startIndex, int length)
        {
            if (startIndex != 0 || s.Length != length)
                s = s.Substring(startIndex, length);

            var value = decimal.Zero;
            if (decimal.TryParse(s, StandardNumberStyles, CultureInfo.InvariantCulture, out value))
                return value;
            else
                return null;
        }

#if !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint Pow10(uint value, int exponent) { return exponent >= 0 ? value * UInt32Powers10[exponent] : value / UInt32Powers10[-exponent]; }
#if !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static ulong Pow10(ulong value, int exponent) { return exponent >= 0 ? value * UInt64Powers10[exponent] : value / UInt64Powers10[-exponent]; }
#if !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static float Pow10(float value, int exponent) { return exponent >= 0 ? value * (float)Pow10(exponent) : value / (float)Pow10(-exponent); }
#if !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static double Pow10(double value, int exponent) { return exponent >= 0 ? value * Pow10(exponent) : value / Pow10(-exponent); }

#if !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static double Pow10(int d) { return d < DoublePowers10.Length ? DoublePowers10[d] : Math.Pow(10.0, d); }

        internal static bool IsDigit(char c) { return '0' <= c && c <= '9'; }
        internal static bool IsTerminal(char c) { return c == '\0' || c == ',' || c == '}' || c == ']' || c == ' ' || c == '\n' || c == '\r' || c == '\t' || c == '"'; }
        private static byte ToDigit(char c) { return (byte)(c - '0'); }
    }
}
