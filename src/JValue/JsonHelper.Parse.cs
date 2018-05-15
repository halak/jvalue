using System;
using System.Globalization;

namespace Halak
{
    public static partial class JsonHelper
    {
        private const int Int32Precision = 10;

        public static int ParseInt32NewNew(string s, int index, int defaultValue = default(int))
        {
            const uint BeforeOverflow = uint.MaxValue / 10 - 1;

            var parser = new JsonNumberParser(s, index);

            var value = 0U;
            while (parser.HasIntegerPartDigit && value <= BeforeOverflow)
                value = (value * 10) + parser.ReadDigitInIntegerPart();

            if (parser.IsFinished == false)
            {
                if (parser.HasIntegerPartDigit)
                {
                    value *= 10;
                    do
                    {
                        parser.ReadDigitInIntegerPart();
                    } while (parser.HasIntegerPartDigit);
                }

                var exponent = 0;
                while (parser.HasFractionalPartDigit && value <= BeforeOverflow)
                {
                    value = (value * 10) + parser.ReadDigitInFractionalPart();
                    exponent--;
                }

                while (parser.HasFractionalPartDigit)
                {
                    parser.ReadDigitInFractionalPart();
                    exponent--;
                }

                if (parser.HasExponent)
                    exponent += parser.ReadExponent();

                if (parser.HasError)
                    return defaultValue;

                if (exponent != 0)
                {
                    var integerPart = parser.Digits + exponent;
                    if (integerPart > 0)
                    {
                        if (integerPart <= Int32Precision)
                        {
                            if (exponent > 0)
                                value *= JNumberParser.Pow10ToUInt32(exponent);
                            else
                                value /= JNumberParser.Pow10ToUInt32(-exponent);
                        }
                        else
                            return defaultValue;
                    }
                    else
                        return 0;
                }
            }

            if (parser.HasError)
                return defaultValue;

            if (parser.IsPositive)
                return value <= int.MaxValue ? (int)value : defaultValue;
            else
            {
                if (value < unchecked((uint)int.MinValue))
                    return -(int)value;
                else if (value == unchecked((uint)int.MinValue))
                    return int.MinValue;
                else
                    return defaultValue;
            }
        }

        public static int ParseInt32New(string s, int startIndex, int length, int defaultValue = default(int))
        {
            return JNumberParser.Analyze(s, startIndex, length).ToInt32(defaultValue);
        }

        private static bool IsInteger(string source, int startIndex, int length)
        {
            for (var i = startIndex; i < startIndex + length; i++)
            {
                switch (source[i])
                {
                    case '.':
                    case 'e':
                    case 'E':
                        return false;
                }
            }

            return true;
        }

        public static int ParseInt32Old(string s, int startIndex, int length, int defaultValue = default(int))
        {
            if (IsInteger(s, startIndex, length) == false)
                return defaultValue;
            if (length <= 0)
                return defaultValue;

            length += startIndex;

            var result = 0;
            if (s[startIndex] != '-')
            {
                if (s[startIndex] == '+')
                    startIndex++;

                for (var i = startIndex; i < length; i++)
                {
                    if ('0' <= s[i] && s[i] <= '9')
                    {
                        result = (result * 10) + (s[i] - '0');
                        if (result < 0) // overflow
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                }
            }
            else
            {
                for (var i = startIndex + 1; i < length; i++)
                {
                    if ('0' <= s[i] && s[i] <= '9')
                    {
                        result = (result * 10) - (s[i] - '0');
                        if (result > 0)  // underflow
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                }
            }

            return result;
        }

        public static int ParseInt32(string s, int startIndex, int length, int defaultValue = default(int))
        {
            if (length <= 0)
                return defaultValue;

            length += startIndex;

            var result = 0;
            if (s[startIndex] != '-')
            {
                if (s[startIndex] == '+')
                    startIndex++;

                for (var i = startIndex; i < length; i++)
                {
                    if ('0' <= s[i] && s[i] <= '9')
                    {
                        result = (result * 10) + (s[i] - '0');
                        if (result < 0) // overflow
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                }
            }
            else
            {
                for (var i = startIndex + 1; i < length; i++)
                {
                    if ('0' <= s[i] && s[i] <= '9')
                    {
                        result = (result * 10) - (s[i] - '0');
                        if (result > 0)  // underflow
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                }
            }

            return result;
        }

        public static long ParseInt64(string s, int startIndex, int length, long defaultValue = default(long))
        {
            if (length <= 0)
                return defaultValue;

            length += startIndex;

            var result = 0L;
            if (s[startIndex] != '-')
            {
                if (s[startIndex] == '+')
                    startIndex++;

                for (var i = startIndex; i < length; i++)
                {
                    if ('0' <= s[i] && s[i] <= '9')
                    {
                        result = (result * 10L) + (s[i] - '0');
                        if (result < 0L) // overflow
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                }
            }
            else
            {
                for (var i = startIndex + 1; i < length; i++)
                {
                    if ('0' <= s[i] && s[i] <= '9')
                    {
                        result = (result * 10L) - (s[i] - '0');
                        if (result > 0L)  // underflow
                            return defaultValue;
                    }
                    else
                        return defaultValue;
                }
            }

            return result;
        }

        public static float ParseSingle(string s, int startIndex, int length, float defaultValue = default(float))
        {
            return (float)ParseDouble(s, startIndex, length, (double)defaultValue);
        }

        public static double ParseDouble(string s, int startIndex, int length, double defaultValue = default(double))
        {
            if (length <= 0)
                return defaultValue;

            var i = startIndex;
            if (s[startIndex] == '-' || s[startIndex] == '+')
                i++;

            length += startIndex;  // length => end
            var mantissa = 0L;
            if (i < length && '0' <= s[i] && s[i] <= '9')
            {
                mantissa = (mantissa * 10) + (s[i] - '0');
                i++;
            }
            else
                return defaultValue;

            for (; i < length; i++)
            {
                if ('0' <= s[i] && s[i] <= '9')
                    mantissa = (mantissa * 10) + (s[i] - '0');
                else if (s[i] == '.' || s[i] == 'e' || s[i] == 'E')
                    break;
                else
                    return defaultValue;
            }

            var exponent = 0;
            if (i < length && s[i] == '.')
            {
                i++;
                for (; i < length; i++, exponent++)
                {
                    if ('0' <= s[i] && s[i] <= '9')
                        mantissa = (mantissa * 10) + (s[i] - '0');
                    else if (s[i] == 'e' || s[i] == 'E')
                        break;
                    else
                        return defaultValue;
                }
            }

            if (i < length)
                exponent -= ParseInt32(s, i + 1, length - (i + 1), 0);

            // defaultValue => result
            if (exponent != 0)
                defaultValue = mantissa / Pow10(exponent);
            else
                defaultValue = mantissa;

            if (s[startIndex] == '-')
                defaultValue = -defaultValue;

            return defaultValue;
        }

        public static decimal ParseDecimal(string s, int startIndex, int length, decimal defaultValue = default(decimal))
        {
            if (startIndex != 0 || s.Length != length)
                s = s.Substring(startIndex, length);

            var styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;
            var value = decimal.Zero;
            if (decimal.TryParse(s, styles, CultureInfo.InvariantCulture, out value))
                return value;
            else
                return defaultValue;
        }

        private static double Pow10(int d) => (((d + Power10Bias) & int.MaxValue) < Power10Count ? Power10[d + Power10Bias] : Pow10Actually(d));
        private static double Pow10Actually(int d) => Math.Pow(10.0, d);

        private const int Power10Bias = 12;
        private const int Power10Count = 24;
        private static readonly double[] Power10 = new[]
        {
            1E-12, 1E-11, 1E-10, 1E-9, 1E-8, 1E-7, 1E-6, 1E-5, 1E-4, 1E-3, 1E-2, 1E-1,
            1E+0, 1E+1, 1E+2, 1E+3, 1E+4, 1E+5, 1E+6, 1E+7, 1E+8, 1E+9, 1E+10, 1E+11,
        };
    }
}
