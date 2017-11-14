using System;
using System.Collections.Generic;
using System.Text;

namespace Halak
{
    public static partial class JsonHelper
    {
        public static int Parse(string s, int startIndex, int length, int defaultValue)
        {
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

        public static long Parse(string s, int startIndex, int length, long defaultValue)
        {
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

        public static float Parse(string s, int startIndex, int length, float defaultValue)
        {
            return (float)Parse(s, startIndex, length, (double)defaultValue);
        }

        public static double Parse(string s, int startIndex, int length, double defaultValue)
        {
            var i = startIndex;
            if (s[startIndex] == '-' || s[startIndex] == '+')
                i++;

            var mantissa = 0L;
            length += startIndex;  // length => end
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
                exponent -= Parse(s, i + 1, length - (i + 1), 0);

            // defaultValue => result
            if (exponent != 0)
                defaultValue = mantissa / Math.Pow(10.0, exponent);
            else
                defaultValue = mantissa;
            if (s[startIndex] == '-')
                defaultValue = -defaultValue;

            return defaultValue;
        }
    }
}
