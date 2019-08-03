using System.IO;
using System.Runtime.CompilerServices;

namespace Halak
{
    public static partial class JsonHelper
    {
        private const string HexChars = "0123456789ABCDEF";
        private static readonly string Int32MinValue = int.MinValue.ToString();
        private static readonly string Int64MinValue = long.MinValue.ToString();
        
        public const string NullString = "null";
        public const string TrueString = "true";
        public const string FalseString = "false";

        public static void WriteInt32(TextWriter writer, int value)
        {
            if (value != 0)
            {
                if (value < 0)
                {
                    if (value == int.MinValue)
                    {
                        writer.Write(Int32MinValue);
                        return;
                    }

                    value = -value;
                    writer.Write('-');
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

                E9: writer.Write((char)('0' + ((value / 1000000000) % 10)));
                E8: writer.Write((char)('0' + ((value / 100000000) % 10)));
                E7: writer.Write((char)('0' + ((value / 10000000) % 10)));
                E6: writer.Write((char)('0' + ((value / 1000000) % 10)));
                E5: writer.Write((char)('0' + ((value / 100000) % 10)));
                E4: writer.Write((char)('0' + ((value / 10000) % 10)));
                E3: writer.Write((char)('0' + ((value / 1000) % 10)));
                E2: writer.Write((char)('0' + ((value / 100) % 10)));
                E1: writer.Write((char)('0' + ((value / 10) % 10)));
                E0: writer.Write((char)('0' + ((value / 1) % 10)));
            }
            else
                writer.Write('0');
        }

        public static void WriteInt64(TextWriter writer, long value)
        {
            if (value != 0)
            {
                if (value < 0)
                {
                    if (value == long.MinValue)
                    {
                        writer.Write(Int64MinValue);
                        return;
                    }

                    value = -value;
                    writer.Write('-');
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

                E18: writer.Write((char)('0' + ((value / 1000000000000000000L) % 10)));
                E17: writer.Write((char)('0' + ((value / 100000000000000000L) % 10)));
                E16: writer.Write((char)('0' + ((value / 10000000000000000L) % 10)));
                E15: writer.Write((char)('0' + ((value / 1000000000000000L) % 10)));
                E14: writer.Write((char)('0' + ((value / 100000000000000L) % 10)));
                E13: writer.Write((char)('0' + ((value / 10000000000000L) % 10)));
                E12: writer.Write((char)('0' + ((value / 1000000000000L) % 10)));
                E11: writer.Write((char)('0' + ((value / 100000000000L) % 10)));
                E10: writer.Write((char)('0' + ((value / 10000000000L) % 10)));
                E9: writer.Write((char)('0' + ((value / 1000000000L) % 10)));
                E8: writer.Write((char)('0' + ((value / 100000000L) % 10)));
                E7: writer.Write((char)('0' + ((value / 10000000L) % 10)));
                E6: writer.Write((char)('0' + ((value / 1000000L) % 10)));
                E5: writer.Write((char)('0' + ((value / 100000L) % 10)));
                E4: writer.Write((char)('0' + ((value / 10000L) % 10)));
                E3: writer.Write((char)('0' + ((value / 1000L) % 10)));
                E2: writer.Write((char)('0' + ((value / 100L) % 10)));
                E1: writer.Write((char)('0' + ((value / 10L) % 10)));
                E0: writer.Write((char)('0' + ((value / 1L) % 10)));
            }
            else
                writer.Write('0');
        }

        internal static void WriteEscapedString(TextWriter writer, string value)
        {
            if (value != null)
            {
                writer.Write('"');
                for (var i = 0; i < value.Length; i++)
                {
                    var c = value[i];
                    if (c > '\u001F')  // is not control char
                    {
                        switch (c)
                        {
                            case '"': WriteEscapedChar(writer, '"'); break;
                            case '\\': WriteEscapedChar(writer, '\\'); break;
                            default: writer.Write(c); break;
                        }
                    }
                    else
                    {
                        switch (c)
                        {
                            case '\n': WriteEscapedChar(writer, 'n'); break;
                            case '\t': WriteEscapedChar(writer, 't'); break;
                            case '\r': WriteEscapedChar(writer, 'r'); break;
                            case '\b': WriteEscapedChar(writer, 'b'); break;
                            case '\f': WriteEscapedChar(writer, 'f'); break;
                            default: WriteHexChar(writer, c); break;
                        }
                    }
                }
                writer.Write('"');
            }
            else
                writer.Write("null");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteEscapedChar(TextWriter writer, char value)
        {
            writer.Write('\\');
            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteHexChar(TextWriter writer, char value)
        {
            writer.Write('\\');
            writer.Write('u');
            writer.Write(HexChars[(value & 0xF000) >> 12]);
            writer.Write(HexChars[(value & 0x0F00) >> 8]);
            writer.Write(HexChars[(value & 0x00F0) >> 4]);
            writer.Write(HexChars[(value & 0x000F) >> 0]);
        }
    }
}
