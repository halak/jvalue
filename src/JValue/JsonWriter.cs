using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Halak
{
    internal sealed class JsonWriter : IDisposable
    {
        private TextWriter underlyingWriter;
        private int offset;

        public JsonWriter(int capacity)
            : this(new StringBuilder(capacity)) { }
        public JsonWriter(StringBuilder builder)
            : this(new StringWriter(builder, CultureInfo.InvariantCulture)) { }
        public JsonWriter(TextWriter writer)
        {
            this.underlyingWriter = writer;
            this.offset = 0;
        }

        public int Offset => offset;

        public void Dispose()
        {
            var disposingWriter = underlyingWriter;
            underlyingWriter = null;
            if (disposingWriter != null)
                disposingWriter.Dispose();
        }

        public void WriteStartArray() => underlyingWriter.Write('[');
        public void WriteEndArray() => underlyingWriter.Write(']');
        public void WriteStartObject() => underlyingWriter.Write('{');
        public void WriteEndObject() => underlyingWriter.Write('}');

        public void WriteNull() => underlyingWriter.Write(JValue.NullLiteral);
        public void Write(bool value) => underlyingWriter.Write(value ? JValue.TrueLiteral : JValue.FalseLiteral);
        public void Write(int value) => underlyingWriter.WriteInt32(value);
        public void Write(long value) => underlyingWriter.WriteInt64(value);
        public void Write(float value) => underlyingWriter.Write(value.ToString(NumberFormatInfo.InvariantInfo));
        public void Write(double value) => underlyingWriter.Write(value.ToString(NumberFormatInfo.InvariantInfo));
        public void Write(decimal value) => underlyingWriter.Write(value.ToString(NumberFormatInfo.InvariantInfo));
        public void Write(string value) => underlyingWriter.WriteEscapedString(value);
        public void Write(JValue value) => value.WriteTo(underlyingWriter);

        public void WriteCommaIf(int offset)
        {
            if (this.offset != offset)
                underlyingWriter.Write(',');

            this.offset++;
        }

        public void WriteKey(string key)
        {
            underlyingWriter.WriteEscapedString(key);
            underlyingWriter.Write(':');
        }

        public JValue BuildJson()
        {
            if (underlyingWriter is StringWriter stringWriter)
            {
                var stringBuilder = stringWriter.GetStringBuilder();
                return new JValue(stringBuilder.ToString(), 0, stringBuilder.Length);
            }
            else
                throw new InvalidOperationException();
        }
    }

    internal static class JsonTextWriterExtensions
    {
        public static void WriteInt32(this TextWriter writer, int value)
        {
            if (value != 0)
            {
                if (value < 0)
                {
                    if (value == int.MinValue)
                    {
                        writer.Write(int.MinValue);
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

        public static void WriteInt64(this TextWriter writer, long value)
        {
            if (value != 0)
            {
                if (value < 0)
                {
                    if (value == long.MinValue)
                    {
                        writer.Write(long.MinValue);
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

        public static void WriteEscapedString(this TextWriter writer, string value)
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
                writer.Write(JValue.NullLiteral);
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
            const string HexChars = "0123456789ABCDEF";

            writer.Write('\\');
            writer.Write('u');
            writer.Write(HexChars[(value & 0xF000) >> 12]);
            writer.Write(HexChars[(value & 0x0F00) >> 8]);
            writer.Write(HexChars[(value & 0x00F0) >> 4]);
            writer.Write(HexChars[(value & 0x000F) >> 0]);
        }
    }
}
