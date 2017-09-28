using System;
using System.Collections.Generic;
using System.Text;

namespace Halak
{
    public static partial class JsonHelper
    {
        internal static void EscapeTo(StringBuilder builder, string value)
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

        internal static char Unescape(string source, ref int index)
        {
            if (source[index] != '\\')
            {
                return source[index];
            }
            else
            {
                index++;

                switch (source[index])
                {
                    case '"': return '"';
                    case '/': return '/';
                    case '\\': return '\\';
                    case 'n': return '\n';
                    case 't': return '\t';
                    case 'r': return '\r';
                    case 'b': return '\b';
                    case 'f': return '\f';
                    case 'u':
                        var a = source[++index];
                        var b = source[++index];
                        var c = source[++index];
                        var d = source[++index];
                        return (char)((Hex(a) * 4096) + (Hex(b) * 256) + (Hex(c) * 16) + (Hex(d)));
                    default:
                        return source[index];
                }
            }
        }

        private static int Hex(char c)
        {
            return
                ('0' <= c && c <= '9') ?
                    c - '0' :
                ('a' <= c && c <= 'f') ?
                    c - 'a' + 10 :
                    c - 'A' + 10;
        }
    }
}
