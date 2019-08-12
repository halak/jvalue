using System;
using System.Collections.Generic;
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

        public int Offset => offset;    

        public JsonWriter(int capacity)
            : this(new StringBuilder(capacity)) { }
        public JsonWriter(StringBuilder builder)
            : this(new StringWriter(builder, CultureInfo.InvariantCulture)) { }
        public JsonWriter(TextWriter writer)
        {
            this.underlyingWriter = writer;
            this.offset = 0;
        }

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

        public void WriteNull() => underlyingWriter.Write(JsonHelper.NullLiteral);
        public void Write(bool value) => underlyingWriter.Write(value ? JsonHelper.TrueLiteral : JsonHelper.FalseLiteral);
        public void Write(int value) => JsonHelper.WriteInt32(underlyingWriter, value);
        public void Write(long value) => JsonHelper.WriteInt64(underlyingWriter, value);
        public void Write(float value) => underlyingWriter.Write(value.ToString(NumberFormatInfo.InvariantInfo));
        public void Write(double value) => underlyingWriter.Write(value.ToString(NumberFormatInfo.InvariantInfo));
        public void Write(decimal value) => underlyingWriter.Write(value.ToString(NumberFormatInfo.InvariantInfo));
        public void Write(string value) => JsonHelper.WriteEscapedString(underlyingWriter, value);
        public void Write(JValue value) => value.WriteTo(underlyingWriter);

        public void WriteCommaIf(int offset)
        {
            if (this.offset != offset)
                underlyingWriter.Write(',');

            this.offset++;
        }

        public void WriteKey(string key)
        {
            JsonHelper.WriteEscapedString(underlyingWriter, key);
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
}
