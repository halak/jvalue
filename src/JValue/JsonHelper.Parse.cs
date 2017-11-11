using System;
using System.Collections.Generic;
using System.Text;

namespace Halak
{
    public static partial class JsonHelper
    {
        public static int Parse(string s, int startIndex, int length, int defaultValue)
        {
            var i = startIndex;
            if (s[startIndex] == '-' || s[startIndex] == '+')
                i++;

            var result = 0L;
            length += startIndex;
            for (; i < length; i++)
            {
                if ('0' <= s[i] && s[i] <= '9')
                    result = (result * 10L) + (s[i] - '0');
                else
                    return defaultValue;
            }

            if (s[startIndex] == '-')
                result = -result;

            if (int.MinValue <= result && result <= int.MaxValue)
                return (int)result;
            else
                return defaultValue;
        }

        public static long Parse(string s, int startIndex, int length, long defaultValue)
        {
            var i = startIndex;
            if (s[startIndex] == '-' || s[startIndex] == '+')
                i++;

            var result = 0L;
            length += startIndex;
            for (; i < length; i++)
            {
                if ('0' <= s[i] && s[i] <= '9')
                {
                    result = (result * 10) + (s[i] - '0');

                    // long이 overflow할 정도 값이면
                    // 이미 제대로된 이 Library에서 수용 가능한 JSON이 아니기 때문에,
                    // overflow를 검사하지 않습니다.
                }
                else
                    return defaultValue;
            }

            if (s[startIndex] == '-')
                result = -result;

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
