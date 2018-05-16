using System;
using System.Text;

namespace Halak
{
    public static partial class JsonHelper
    {
        private static readonly string Int32MinValue = int.MinValue.ToString();
        private static readonly string Int64MinValue = long.MinValue.ToString();

        public const string NullString = "null";
        public const string TrueString = "true";
        public const string FalseString = "false";

        public static void AppendInt32(StringBuilder builder, int value)
        {
            if (value != 0)
            {
                if (value < 0)
                {
                    if (value == int.MinValue)
                    {
                        builder.Append(Int32MinValue);
                        return;
                    }

                    value = -value;
                    builder.Append('-');
                }

                if (value < 10) goto E0;
                else if (value < 100) goto E1;
                else if (value < 1000) goto E2;
                else if (value < 10000) goto E3;
                else if (value < 100000) goto E4;
                else if (value < 1000000) goto E5;
                else if (value < 10000000) goto E6;
                else if (value < 100000000) goto E7;
                else if (value < 1000000000) goto E8;
                else goto E9;

                E9: builder.Append((char)('0' + ((value / 1000000000) % 10)));
                E8: builder.Append((char)('0' + ((value / 100000000) % 10)));
                E7: builder.Append((char)('0' + ((value / 10000000) % 10)));
                E6: builder.Append((char)('0' + ((value / 1000000) % 10)));
                E5: builder.Append((char)('0' + ((value / 100000) % 10)));
                E4: builder.Append((char)('0' + ((value / 10000) % 10)));
                E3: builder.Append((char)('0' + ((value / 1000) % 10)));
                E2: builder.Append((char)('0' + ((value / 100) % 10)));
                E1: builder.Append((char)('0' + ((value / 10) % 10)));
                E0: builder.Append((char)('0' + ((value / 1) % 10)));
            }
            else
                builder.Append('0');
        }

        public static void AppendInt64(StringBuilder builder, long value)
        {
            if (value != 0)
            {
                if (value < 0)
                {
                    if (value == long.MinValue)
                    {
                        builder.Append(Int64MinValue);
                        return;
                    }

                    value = -value;
                    builder.Append('-');
                }

                if (value < 10L) goto E0;
                else if (value < 100L) goto E1;
                else if (value < 1000L) goto E2;
                else if (value < 10000L) goto E3;
                else if (value < 100000L) goto E4;
                else if (value < 1000000L) goto E5;
                else if (value < 10000000L) goto E6;
                else if (value < 100000000L) goto E7;
                else if (value < 1000000000L) goto E8;
                else if (value < 10000000000L) goto E9;
                else if (value < 100000000000L) goto E10;
                else if (value < 1000000000000L) goto E11;
                else if (value < 10000000000000L) goto E12;
                else if (value < 100000000000000L) goto E13;
                else if (value < 1000000000000000L) goto E14;
                else if (value < 10000000000000000L) goto E15;
                else if (value < 100000000000000000L) goto E16;
                else if (value < 1000000000000000000L) goto E17;
                else goto E18;

                E18: builder.Append((char)('0' + ((value / 1000000000000000000L) % 10)));
                E17: builder.Append((char)('0' + ((value / 100000000000000000L) % 10)));
                E16: builder.Append((char)('0' + ((value / 10000000000000000L) % 10)));
                E15: builder.Append((char)('0' + ((value / 1000000000000000L) % 10)));
                E14: builder.Append((char)('0' + ((value / 100000000000000L) % 10)));
                E13: builder.Append((char)('0' + ((value / 10000000000000L) % 10)));
                E12: builder.Append((char)('0' + ((value / 1000000000000L) % 10)));
                E11: builder.Append((char)('0' + ((value / 100000000000L) % 10)));
                E10: builder.Append((char)('0' + ((value / 10000000000L) % 10)));
                E9: builder.Append((char)('0' + ((value / 1000000000L) % 10)));
                E8: builder.Append((char)('0' + ((value / 100000000L) % 10)));
                E7: builder.Append((char)('0' + ((value / 10000000L) % 10)));
                E6: builder.Append((char)('0' + ((value / 1000000L) % 10)));
                E5: builder.Append((char)('0' + ((value / 100000L) % 10)));
                E4: builder.Append((char)('0' + ((value / 10000L) % 10)));
                E3: builder.Append((char)('0' + ((value / 1000L) % 10)));
                E2: builder.Append((char)('0' + ((value / 100L) % 10)));
                E1: builder.Append((char)('0' + ((value / 10L) % 10)));
                E0: builder.Append((char)('0' + ((value / 1L) % 10)));
            }
            else
                builder.Append('0');
        }

        internal static void AppendEscapedString(StringBuilder builder, string value)
        {
            if (value != null)
            {
                builder.Append('"');
                for (var i = 0; i < value.Length; i++)
                {
                    switch (value[i])
                    {
                        case '"': builder.Append('\\'); builder.Append('"'); break;
                        case '\\': builder.Append('\\'); builder.Append('\\'); break;
                        case '\n': builder.Append('\\'); builder.Append('n'); break;
                        case '\t': builder.Append('\\'); builder.Append('t'); break;
                        case '\r': builder.Append('\\'); builder.Append('r'); break;
                        case '\b': builder.Append('\\'); builder.Append('b'); break;
                        case '\f': builder.Append('\\'); builder.Append('f'); break;
                        default: builder.Append(value[i]); break;
                    }
                }
                builder.Append('"');
            }
            else
                builder.Append("null");
        }
    }
}
