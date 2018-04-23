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
    }
}
