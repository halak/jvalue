using System;
using System.Globalization;

namespace Halak
{
    partial struct JNumber
    {
        public static int ParseInt32(string s, int defaultValue = default(int))
            => ParseInt32(s, 0, s.Length, defaultValue);

        public static int ParseInt32(string s, int startIndex, int defaultValue = default(int))
            => ParseInt32(s, startIndex, s.Length, defaultValue);

        public static int ParseInt32(string s, int startIndex, int length, int defaultValue = default(int))
        {
            const int Zero = default(int);
            const uint BeforeOverflow = uint.MaxValue / 10 - 1;
            const uint MaxPositiveValue = int.MaxValue;
            const uint MaxNegativeValue = unchecked((uint)int.MinValue);

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return defaultValue;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(uint);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }

                if (index == endIndex || IsTerminal(s[index]))
                {
                    if (isPositive)
                        return (mantissa <= MaxPositiveValue) ? (int)(mantissa) : defaultValue;
                    else
                        return (mantissa <= MaxNegativeValue) ? (int)(0 - mantissa) : defaultValue;
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return defaultValue;  // unexpected character
            }
            else
                return defaultValue;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            digits += exponent;
            if (digits <= 0)
                return Zero;
            else if (digits > 10)
                return defaultValue;  // overflow
            else
            {
                if (exponent != 0)
                    mantissa = Pow10(mantissa, exponent);

                if (isPositive)
                    return (mantissa <= MaxPositiveValue) ? (int)(mantissa) : defaultValue;
                else
                    return (mantissa <= MaxNegativeValue) ? (int)(0 - mantissa) : defaultValue;
            }
        }

        public static int? ParseNullableInt32(string s)
            => ParseNullableInt32(s, 0, s.Length);

        public static int? ParseNullableInt32(string s, int startIndex)
            => ParseNullableInt32(s, startIndex, s.Length);

        public static int? ParseNullableInt32(string s, int startIndex, int length)
        {
            const int Zero = default(int);
            const uint BeforeOverflow = uint.MaxValue / 10 - 1;
            const uint MaxPositiveValue = int.MaxValue;
            const uint MaxNegativeValue = unchecked((uint)int.MinValue);

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return null;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(uint);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }

                if (index == endIndex || IsTerminal(s[index]))
                {
                    if (isPositive)
                        return (mantissa <= MaxPositiveValue) ? (int?)(mantissa) : null;
                    else
                        return (mantissa <= MaxNegativeValue) ? (int?)(0 - mantissa) : null;
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return null;  // unexpected character
            }
            else
                return null;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            digits += exponent;
            if (digits <= 0)
                return Zero;
            else if (digits > 10)
                return null;  // overflow
            else
            {
                if (exponent != 0)
                    mantissa = Pow10(mantissa, exponent);

                if (isPositive)
                    return (mantissa <= MaxPositiveValue) ? (int?)(mantissa) : null;
                else
                    return (mantissa <= MaxNegativeValue) ? (int?)(0 - mantissa) : null;
            }
        }

        public static long ParseInt64(string s, long defaultValue = default(long))
            => ParseInt64(s, 0, s.Length, defaultValue);

        public static long ParseInt64(string s, int startIndex, long defaultValue = default(long))
            => ParseInt64(s, startIndex, s.Length, defaultValue);

        public static long ParseInt64(string s, int startIndex, int length, long defaultValue = default(long))
        {
            const long Zero = default(long);
            const ulong BeforeOverflow = ulong.MaxValue / 10 - 1;
            const ulong MaxPositiveValue = long.MaxValue;
            const ulong MaxNegativeValue = unchecked((ulong)long.MinValue);

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return defaultValue;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(ulong);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }

                if (index == endIndex || IsTerminal(s[index]))
                {
                    if (isPositive)
                        return (mantissa <= MaxPositiveValue) ? (long)(mantissa) : defaultValue;
                    else
                        return (mantissa <= MaxNegativeValue) ? (long)(0 - mantissa) : defaultValue;
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return defaultValue;  // unexpected character
            }
            else
                return defaultValue;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            digits += exponent;
            if (digits <= 0)
                return Zero;
            else if (digits > 20)
                return defaultValue;  // overflow
            else
            {
                if (exponent != 0)
                    mantissa = Pow10(mantissa, exponent);

                if (isPositive)
                    return (mantissa <= MaxPositiveValue) ? (long)(mantissa) : defaultValue;
                else
                    return (mantissa <= MaxNegativeValue) ? (long)(0 - mantissa) : defaultValue;
            }
        }

        public static long? ParseNullableInt64(string s)
            => ParseNullableInt64(s, 0, s.Length);

        public static long? ParseNullableInt64(string s, int startIndex)
            => ParseNullableInt64(s, startIndex, s.Length);

        public static long? ParseNullableInt64(string s, int startIndex, int length)
        {
            const long Zero = default(long);
            const ulong BeforeOverflow = ulong.MaxValue / 10 - 1;
            const ulong MaxPositiveValue = long.MaxValue;
            const ulong MaxNegativeValue = unchecked((ulong)long.MinValue);

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return null;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(ulong);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }

                if (index == endIndex || IsTerminal(s[index]))
                {
                    if (isPositive)
                        return (mantissa <= MaxPositiveValue) ? (long?)(mantissa) : null;
                    else
                        return (mantissa <= MaxNegativeValue) ? (long?)(0 - mantissa) : null;
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return null;  // unexpected character
            }
            else
                return null;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            digits += exponent;
            if (digits <= 0)
                return Zero;
            else if (digits > 20)
                return null;  // overflow
            else
            {
                if (exponent != 0)
                    mantissa = Pow10(mantissa, exponent);

                if (isPositive)
                    return (mantissa <= MaxPositiveValue) ? (long?)(mantissa) : null;
                else
                    return (mantissa <= MaxNegativeValue) ? (long?)(0 - mantissa) : null;
            }
        }

        public static float ParseSingle(string s, float defaultValue = default(float))
            => ParseSingle(s, 0, s.Length, defaultValue);

        public static float ParseSingle(string s, int startIndex, float defaultValue = default(float))
            => ParseSingle(s, startIndex, s.Length, defaultValue);

        public static float ParseSingle(string s, int startIndex, int length, float defaultValue = default(float))
        {
            const float Zero = default(float);
            const uint BeforeOverflow = uint.MaxValue / 10 - 1;

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return defaultValue;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(uint);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return defaultValue;  // unexpected character
            }
            else
                return defaultValue;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            if (digits >= 10)
            {
                var substring = s.Substring(startIndex, index - startIndex - 1);
                var formatProvider = NumberFormatInfo.InvariantInfo;
                if (float.TryParse(substring, StandardNumberStyles, formatProvider, out var number))
                    return number;
                else
                    return defaultValue;
            }

            var value = exponent != 0 ? Pow10((float)mantissa, exponent) : mantissa;
            if (float.IsInfinity(value) == false && float.IsNaN(value) == false)
                return isPositive ? value : -value;
            else
                return defaultValue;  // overflow
        }

        public static float? ParseNullableSingle(string s)
            => ParseNullableSingle(s, 0, s.Length);

        public static float? ParseNullableSingle(string s, int startIndex)
            => ParseNullableSingle(s, startIndex, s.Length);

        public static float? ParseNullableSingle(string s, int startIndex, int length)
        {
            const float Zero = default(float);
            const uint BeforeOverflow = uint.MaxValue / 10 - 1;

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return null;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(uint);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return null;  // unexpected character
            }
            else
                return null;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            if (digits >= 10)
            {
                var substring = s.Substring(startIndex, index - startIndex - 1);
                var formatProvider = NumberFormatInfo.InvariantInfo;
                if (float.TryParse(substring, StandardNumberStyles, formatProvider, out var number))
                    return number;
                else
                    return null;
            }

            var value = exponent != 0 ? Pow10((float)mantissa, exponent) : mantissa;
            if (float.IsInfinity(value) == false && float.IsNaN(value) == false)
                return isPositive ? value : -value;
            else
                return null;  // overflow
        }

        public static double ParseDouble(string s, double defaultValue = default(double))
            => ParseDouble(s, 0, s.Length, defaultValue);

        public static double ParseDouble(string s, int startIndex, double defaultValue = default(double))
            => ParseDouble(s, startIndex, s.Length, defaultValue);

        public static double ParseDouble(string s, int startIndex, int length, double defaultValue = default(double))
        {
            const double Zero = default(double);
            const ulong BeforeOverflow = ulong.MaxValue / 10 - 1;

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return defaultValue;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(ulong);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return defaultValue;  // unexpected character
            }
            else
                return defaultValue;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return defaultValue;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            if (digits >= 20)
            {
                var substring = s.Substring(startIndex, index - startIndex - 1);
                var formatProvider = NumberFormatInfo.InvariantInfo;
                if (double.TryParse(substring, StandardNumberStyles, formatProvider, out var number))
                    return number;
                else
                    return defaultValue;
            }

            var value = exponent != 0 ? Pow10((double)mantissa, exponent) : mantissa;
            if (double.IsInfinity(value) == false && double.IsNaN(value) == false)
                return isPositive ? value : -value;
            else
                return defaultValue;  // overflow
        }

        public static double? ParseNullableDouble(string s)
            => ParseNullableDouble(s, 0, s.Length);

        public static double? ParseNullableDouble(string s, int startIndex)
            => ParseNullableDouble(s, startIndex, s.Length);

        public static double? ParseNullableDouble(string s, int startIndex, int length)
        {
            const double Zero = default(double);
            const ulong BeforeOverflow = ulong.MaxValue / 10 - 1;

            var endIndex = Math.Min(startIndex + length, s.Length);
            if (length <= 0)
                return null;  // empty string

            var index = s[startIndex] == '-' ? startIndex + 1 : startIndex;
            var firstIntegerPartDigitIndex = index;
            var isPositive = index == startIndex;

            var mantissa = default(ulong);
            var c = s[index];
            if ('1' <= c && c <= '9')
            {
                mantissa = ToDigit(c);
                index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipIntegerPart(s, endIndex, index + 1);
                            c = (index < endIndex) ? s[index] : '\0';
                            break;
                        }
                    }
                    else if (c == '.' || c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }
            }
            else if (c == '0')
            {
                firstIntegerPartDigitIndex++;

                c = (++index < endIndex) ? s[index] : '\0';
                if (c == '.' || c == 'e' || c == 'E')
                { }
                else if (IsTerminal(c))
                    return Zero;
                else
                    return null;  // unexpected character
            }
            else
                return null;  // unexpected character

            var decimalPointIndex = -1;
            if (c == '.')
            {
                decimalPointIndex = index++;

                for (; index < endIndex; index++)
                {
                    c = s[index];
                    if (IsDigit(c))
                    {
                        if (mantissa <= BeforeOverflow)
                            mantissa = (mantissa * 10) + ToDigit(c);
                        else
                        {
                            mantissa *= 10;
                            index = SkipFractionalPart(s, endIndex, index + 1);
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E' || IsTerminal(c))
                        break;
                    else
                        return null;  // unexpected character
                }
            }

            var digits = 0;
            var exponent = 0;
            if (decimalPointIndex >= 0)
            {
                digits = index - firstIntegerPartDigitIndex - 1;
                exponent = -(index - (decimalPointIndex + 1));
            }
            else
            {
                digits = index - firstIntegerPartDigitIndex;
                exponent = 0;
            }

            var exponentSignIndex = -1;
            if (c == 'e' || c == 'E')
            {
                exponentSignIndex = index++;
                exponent += ReadExponentIfSmall(s, endIndex, ref index);
            }

            if (digits >= 20)
            {
                var substring = s.Substring(startIndex, index - startIndex - 1);
                var formatProvider = NumberFormatInfo.InvariantInfo;
                if (double.TryParse(substring, StandardNumberStyles, formatProvider, out var number))
                    return number;
                else
                    return null;
            }

            var value = exponent != 0 ? Pow10((double)mantissa, exponent) : mantissa;
            if (double.IsInfinity(value) == false && double.IsNaN(value) == false)
                return isPositive ? value : -value;
            else
                return null;  // overflow
        }
    }
}

