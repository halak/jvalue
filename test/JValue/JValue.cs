using System;
using System.Collections.Generic;
using System.Globalization;

namespace Halak
{
    /// <summary>
    /// Super lightweight JSON Reader
    /// </summary>
    /// <seealso cref="http://www.json.org/"/>
    /// <seealso cref="https://github.com/halak/jvalue/"/>
    public struct JValue : IComparable<JValue>, IEquatable<JValue>
    {
        #region TypeCode
        public enum TypeCode
        {
            Null,
            Boolean,
            Number,
            String,
            Array,
            Object,
        }
        #endregion

        #region Static Fields
        public static readonly JValue Null = new JValue();
        #endregion

        #region Fields
        private readonly string source;
        private readonly int startIndex;
        private readonly int length;
        #endregion

        #region Properties
        public TypeCode Type
        {
            get
            {
                if (source != null && source.Length > 0)
                {
                    switch (source[startIndex])
                    {
                        case '"':
                            return TypeCode.String;
                        case '[':
                            return TypeCode.Array;
                        case '{':
                            return TypeCode.Object;
                        case 't':
                        case 'f':
                            return TypeCode.Boolean;
                        case 'n':
                            return TypeCode.Null;
                        default:
                            return TypeCode.Number;
                    }
                }
                else
                    return TypeCode.Null;
            }
        }
        #endregion

        #region Indexer
        /// <seealso cref="JValue.Get(System.Int32)"/>
        public JValue this[int index]
        {
            get { return Get(index); }
        }

        /// <seealso cref="JValue.Get(System.String)"/>
        public JValue this[string key]
        {
            get { return Get(key); }
        }
        #endregion

        #region Constructors
        public static JValue Parse(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var index = SkipWhitespaces(source);
            return new JValue(source, index, source.Length - index);
        }

        public JValue(bool value)
        {
            source = value ? "true" : "false";
            startIndex = 0;
            length = source.Length;
        }

        public JValue(int value)
        {
            source = value.ToString(CultureInfo.InvariantCulture);
            startIndex = 0;
            length = source.Length;
        }

        public JValue(long value)
        {
            source = value.ToString(CultureInfo.InvariantCulture);
            startIndex = 0;
            length = source.Length;
        }

        public JValue(float value)
        {
            source = value.ToString(CultureInfo.InvariantCulture);
            startIndex = 0;
            length = source.Length;
        }

        public JValue(double value)
        {
            source = value.ToString(CultureInfo.InvariantCulture);
            startIndex = 0;
            length = source.Length;
        }

        public JValue(string value)
        {
            if (value != null)
            {
                var sb = new System.Text.StringBuilder(value.Length + 2);
                sb.Append('"');
                for (var i = 0; i < value.Length; i++)
                {
                    switch (value[i])
                    {
                        case '"': sb.Append('\\'); sb.Append('"'); break;
                        case '\\': sb.Append('\\'); sb.Append('\\'); break;
                        case '\n': sb.Append('\\'); sb.Append('n'); break;
                        case '\t': sb.Append('\\'); sb.Append('t'); break;
                        case '\r': sb.Append('\\'); sb.Append('r'); break;
                        case '\b': sb.Append('\\'); sb.Append('b'); break;
                        case '\f': sb.Append('\\'); sb.Append('f'); break;
                        default: sb.Append(value[i]); break;
                    }
                }
                sb.Append('"');

                source = sb.ToString();
                startIndex = 0;
                length = source.Length;
            }
            else
            {
                source = null;
                startIndex = 0;
                length = 0;
            }
        }

        public JValue(IEnumerable<JValue> value)
        {
            using (var sw = new System.IO.StringWriter(CultureInfo.InvariantCulture))
            {
                Serialize(sw, value, 0);
                source = sw.ToString();
                startIndex = 0;
                length = source.Length;
            }
        }

        public JValue(IEnumerable<KeyValuePair<string, JValue>> value)
        {
            using (var sw = new System.IO.StringWriter(CultureInfo.InvariantCulture))
            {
                Serialize(sw, value, 0);
                source = sw.ToString();
                startIndex = 0;
                length = source.Length;
            }
        }

        private JValue(string source, int startOffset, int length)
        {
            this.source = source;
            this.startIndex = startOffset;
            this.length = length;
        }
        #endregion

        #region Methods
        #region As
        public bool AsBoolean()
        {
            switch (Type)
            {
                case TypeCode.Null:
                    return false;
                case TypeCode.Boolean:
                    return AsBooleanActually();
                case TypeCode.Number:
                    return AsDoubleActually() != 0.0;
                case TypeCode.String:
                    return length != 2;  // two quotation marks
                case TypeCode.Array:
                case TypeCode.Object:
                    return true;
                default:
                    return false;
            }
        }

        private bool AsBooleanActually()
        {
            return source[startIndex] == 't';
        }

        public int AsInt(int defaultValue = 0)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1 : 0;
                case TypeCode.Number:
                    return AsIntActually(defaultValue);
                case TypeCode.String:
                    return JValueExtensions.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private bool IsInteger()
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

        private int AsIntActually(int defaultValue = 0)
        {
            if (IsInteger())
                return JValueExtensions.Parse(source, startIndex, length, defaultValue);
            else
                return (int)JValueExtensions.Parse(source, startIndex, length, (double)defaultValue);
        }

        public long AsLong(long defaultValue = 0)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1 : 0;
                case TypeCode.Number:
                    return AsLongActually(defaultValue);
                case TypeCode.String:
                    return JValueExtensions.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private long AsLongActually(long defaultValue = 0L)
        {
            if (IsInteger())
                return JValueExtensions.Parse(source, startIndex, length, defaultValue);
            else
                return (long)JValueExtensions.Parse(source, startIndex, length, (double)defaultValue);
        }

        public float AsFloat(float defaultValue = 0.0f)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1 : 0;
                case TypeCode.Number:
                    return AsFloatActually(defaultValue);
                case TypeCode.String:
                    return JValueExtensions.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private float AsFloatActually(float defaultValue = 0.0f)
        {
            return JValueExtensions.Parse(source, startIndex, length, defaultValue);
        }

        public double AsDouble(double defaultValue = 0.0)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1 : 0;
                case TypeCode.Number:
                    return AsDoubleActually(defaultValue);
                case TypeCode.String:
                    return JValueExtensions.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private double AsDoubleActually(double defaultValue = 0.0)
        {
            return JValueExtensions.Parse(source, startIndex, length, defaultValue);
        }

        public string AsString(string defaultValue = "")
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? "true" : "false";
                case TypeCode.Number:
                    return source.Substring(startIndex, length);
                case TypeCode.String:
                    return AsStringActually();
                default:
                    return defaultValue;
            }
        }

        private string AsStringActually()
        {
            var sb = new System.Text.StringBuilder(length - 2);
            var end = startIndex + length - 1;
            for (var i = startIndex + 1; i < end; i++)
            {
                if (source[i] != '\\')
                    sb.Append(source[i]);
                else
                {
                    i++;

                    switch (source[i])
                    {
                        case '"': sb.Append('"'); break;
                        case '/': sb.Append('/'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'u':
                            char a = source[++i];
                            char b = source[++i];
                            char c = source[++i];
                            char d = source[++i];
                            sb.Append((char)((Hex(a) * 4096) + (Hex(b) * 256) + (Hex(c) * 16) + (Hex(d))));
                            break;
                    }
                }
            }

            return sb.ToString();
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

        public List<JValue> AsArray()
        {
            var result = new List<JValue>(GetElementCount());
            foreach (var item in Array())
                result.Add(item);

            return result;
        }

        public Dictionary<string, JValue> AsObject()
        {
            var result = new Dictionary<string, JValue>(GetElementCount());
            foreach (var item in Object())
                result[item.Key] = item.Value;

            return result;
        }
        #endregion

        #region Get
        /// <summary>
        /// 입력한 색인에 존재하는 JValue를 가져옵니다.
        /// 만약 색인을 음수로 입력하면 뒤에서부터 가져옵니다.
        /// </summary>
        /// <param name="index">색인</param>
        /// <returns>입력한 색인에 존재하는 JValue 값. 만약 객체가 배열이 아니거나 색인이 범위를 벗어났으면 JValue.Null을 반환합니다.</returns>
        /// <example>
        /// <code>
        /// var x = new JValue("[1,2,3,4,5,6,7,8,9]");
        /// Trace.Assert(x[0] == 1);
        /// Trace.Assert(x[1].AsInt() == 2);
        /// Trace.Assert(x[-1] == 9);
        /// Trace.Assert(x[-2] == 8);
        /// </code>
        /// </example>
        public JValue Get(int index)
        {
            // TODO: OPTIMIZE

            if (Type == TypeCode.Array)
            {
                if (index < 0)
                    index += GetElementCount();

                foreach (var item in Array())
                {
                    if (index-- == 0)
                        return item;
                }
            }

            return Null;
        }

        private int GetElementCount()
        {
            var count = 1;
            var depth = 0;
            var end = startIndex + length - 1;  // ignore } or ]
            for (var i = startIndex + 1; i < end; i++)  // ignore { or [
            {
                switch (source[i])
                {
                    case ',':
                        if (depth == 0)
                            count++;
                        break;
                    case '[':
                    case '{':
                        depth++;
                        break;
                    case ']':
                    case '}':
                        depth--;
                        break;
                    case '"':
                        i = SkipString(i) - 1;
                        break;
                }
            }

            return count;
        }

        /// <summary>
        /// 입력한 이름에 해당하는 하위 JValue를 가져옵니다.
        /// </summary>
        /// <param name="key">이름</param>
        /// <returns>입력한 이름에 존재하는 JValue 값.</returns>
        /// <example>
        /// <code>
        /// var x = JValue.Parse("{hello:{world:10}}");
        /// Trace.Assert(x["hello"]["world"] == 10);
        /// </code>
        /// </example>
        public JValue Get(string key)
        {
            if (Type == TypeCode.Object)
            {
                var end = startIndex + length - 1;

                var kStart = SkipWhitespaces(startIndex + 1);
                while (kStart < end)
                {
                    var kEnd = SkipString(kStart);
                    var vStart = SkipWhitespaces(kEnd + 1);
                    var vEnd = SkipValue(vStart);

                    kStart++; // remove quotes
                    kEnd--; // remove quotes

                    if (EqualsKey(key, source, kStart, key.Length))
                        return new JValue(source, vStart, vEnd - vStart);

                    kStart = SkipWhitespaces(vEnd + 1);
                }
            }

            return JValue.Null;
        }

        private static bool EqualsKey(string s, string key, int startIndex, int length)
        {
            if (s.Length > key.Length)
                return false;

            var end = startIndex + length;
            for (int i = startIndex, k = 0; i < end; i++, k++)
            {
                if (key[i] == '\\')
                {
                    i++;

                    var character = ' ';
                    switch (key[i])
                    {
                        case '"': character = '"'; break;
                        case '/': character = '/'; break;
                        case '\\': character = '\\'; break;
                        case 'n': character = '\n'; break;
                        case 't': character = '\t'; break;
                        case 'r': character = '\r'; break;
                        case 'b': character = '\b'; break;
                        case 'f': character = '\f'; break;
                        case 'u':
                            char a = key[++i];
                            char b = key[++i];
                            char c = key[++i];
                            char d = key[++i];
                            character = (char)((Hex(a) * 4096) + (Hex(b) * 256) + (Hex(c) * 16) + (Hex(d)));
                            break;
                    }

                    if (s[k] != character)
                        return false;
                }
                else
                {
                    if (s[k] != key[i])
                        return false;
                }
            }

            return true;
        }
        #endregion

        #region Enumeration
        public IEnumerable<JValue> Array()
        {
            if (Type == TypeCode.Array)
            {
                var end = startIndex + length - 1;

                var vStart = SkipWhitespaces(startIndex + 1);
                while (vStart < end)
                {
                    var vEnd = SkipValue(vStart);
                    yield return new JValue(source, vStart, vEnd - vStart);
                    vStart = SkipWhitespaces(vEnd + 1);
                }
            }
        }

        public IEnumerable<KeyValuePair<int, JValue>> IndexedArray()
        {
            if (Type == TypeCode.Array)
            {
                var end = startIndex + length - 1;

                var index = 0;
                var vStart = SkipWhitespaces(startIndex + 1);
                while (vStart < end)
                {
                    var vEnd = SkipValue(vStart);
                    yield return new KeyValuePair<int, JValue>(index++, new JValue(source, vStart, vEnd - vStart));
                    vStart = SkipWhitespaces(vEnd + 1);
                }
            }
        }

        public IEnumerable<KeyValuePair<string, JValue>> Object()
        {
            if (Type == TypeCode.Object)
            {
                var end = startIndex + length - 1;

                var kStart = SkipWhitespaces(startIndex + 1);
                while (kStart < end)
                {
                    var kEnd = SkipString(kStart);
                    var vStart = SkipWhitespaces(kEnd + 1);
                    var vEnd = SkipValue(vStart);

                    yield return new KeyValuePair<string, JValue>(new JValue(source, kStart, kEnd - kStart).AsStringActually(),
                                                                  new JValue(source, vStart, vEnd - vStart));
                    kStart = SkipWhitespaces(vEnd + 1);
                }
            }
        }

        private int SkipValue(int index)
        {
            var end = startIndex + length;
            if (end <= index)
                return end;

            switch (source[index])
            {
                case '"':
                    return SkipString(index);
                case '[':
                case '{':
                    return SkipBracket(index);
                default:
                    return SkipLetterOrDigit(index);
            }
        }

        private static int SkipWhitespaces(string source)
        {
            for (var i = 0; i < source.Length; i++)
            {
                switch (source[i])
                {
                    case ' ':
                    case ':':
                    case ',':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        return i;
                }
            }

            return source.Length;
        }

        private int SkipWhitespaces(int index)
        {
            var end = startIndex + length;
            for (; index < end; index++)
            {
                switch (source[index])
                {
                    case ' ':
                    case ':':
                    case ',':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        return index;
                }
            }

            return end;
        }

        private int SkipLetterOrDigit(int index)
        {
            var end = startIndex + length;
            for (; index < end; index++)
            {
                switch (source[index])
                {
                    case ' ':
                    case ':':
                    case ',':
                    case ']':
                    case '}':
                    case '"':
                    case '\t':
                    case '\r':
                    case '\n':
                        return index;
                }
            }

            return end;
        }

        private int SkipString(int index)
        {
            var end = startIndex + length;
            index++;
            for (; index < end; index++)
            {
                switch (source[index])
                {
                    case '"':
                        return index + 1;
                    case '\\':
                        index++;
                        break;
                }
            }

            return end;
        }

        private int SkipBracket(int index)
        {
            var end = startIndex + length;
            var depth = 0;
            for (; index < end; index++)
            {
                switch (source[index])
                {
                    case '[':
                    case '{':
                        depth++;
                        break;
                    case ']':
                    case '}':
                        depth--;

                        if (depth == 0)
                            return index + 1;
                        break;
                    case '"':
                        index = SkipString(index) - 1;
                        break;
                }
            }

            return end;
        }
        #endregion

        #region Serialization
        public string Serialize(bool prettyPrint = false)
        {
            var sw = new System.IO.StringWriter(CultureInfo.InvariantCulture);
            Serialize(sw, prettyPrint);
            return sw.ToString();
        }

        public void Serialize(System.IO.TextWriter writer, bool prettyPrint = false)
        {
            Serialize(writer, this, 0, prettyPrint);
        }

        private static void Spaces(System.IO.TextWriter writer, int spaces)
        {
            for (var i = 0; i < spaces; i++)
                writer.Write(' ');
        }

        private static void Indent(System.IO.TextWriter writer, int indentLevel)
        {
            Spaces(writer, indentLevel * 4);
        }

        private static void Serialize(System.IO.TextWriter writer, JValue value, int indentLevel, bool prettyPrint = false)
        {
            switch (value.Type)
            {
                case TypeCode.Array:
                    Serialize(writer, value.Array(), indentLevel, prettyPrint);
                    break;
                case TypeCode.Object:
                    Serialize(writer, value.Object(), indentLevel, prettyPrint);
                    break;
                default:
                    writer.Write(value.ToString());
                    break;
            }
        }

        private static void Serialize(System.IO.TextWriter writer, IEnumerable<JValue> value, int indentLevel, bool prettyPrint = false)
        {
            if (prettyPrint)
                writer.WriteLine('[');
            else
                writer.Write('[');

            var isFirst = true;
            foreach (var item in value)
            {
                if (isFirst == false)
                {
                    if (prettyPrint)
                        writer.WriteLine(',');
                    else
                        writer.Write(',');
                }
                else
                    isFirst = false;

                if (prettyPrint)
                    Indent(writer, indentLevel + 1);

                Serialize(writer, item, indentLevel + 1, prettyPrint);
            }

            if (prettyPrint)
            {
                writer.WriteLine();
                Indent(writer, indentLevel);
            }

            writer.Write(']');
        }

        private static void Serialize(System.IO.TextWriter writer, IEnumerable<KeyValuePair<string, JValue>> value, int indentLevel, bool prettyPrint = false)
        {
            if (prettyPrint)
                writer.WriteLine('{');
            else
                writer.Write('{');

            var maxKeyLength = 0;
            if (prettyPrint)
            {
                foreach (var item in value)
                    maxKeyLength = Math.Max(maxKeyLength, item.Key.Length);

                maxKeyLength += 1;
            }

            var isFirst = true;
            foreach (var item in value)
            {
                if (isFirst == false)
                {
                    if (prettyPrint)
                        writer.WriteLine(',');
                    else
                        writer.Write(',');
                }
                else
                    isFirst = false;

                if (prettyPrint)
                    Indent(writer, indentLevel + 1);

                writer.Write('"');
                for (var i = 0; i < item.Key.Length; i++)
                {
                    switch (item.Key[i])
                    {
                        case '"': writer.Write('\\'); writer.Write('"'); break;
                        case '\\': writer.Write('\\'); writer.Write('\\'); break;
                        case '\n': writer.Write('\\'); writer.Write('n'); break;
                        case '\t': writer.Write('\\'); writer.Write('t'); break;
                        case '\r': writer.Write('\\'); writer.Write('r'); break;
                        case '\b': writer.Write('\\'); writer.Write('b'); break;
                        case '\f': writer.Write('\\'); writer.Write('f'); break;
                        default: writer.Write(item.Key[i]); break;
                    }
                }
                writer.Write('"');
                writer.Write(':');
                if (prettyPrint)
                    Spaces(writer, maxKeyLength - item.Key.Length);
                Serialize(writer, item.Value, indentLevel + 1, prettyPrint);
            }

            if (prettyPrint)
            {
                writer.WriteLine();
                Indent(writer, indentLevel);
            }

            writer.Write('}');
        }
        #endregion

        #region Object
        public override int GetHashCode()
        {
            return source.GetHashCode() + startIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is JValue)
                return Equals((JValue)obj);
            else
                return false;
        }

        public override string ToString()
        {
            if (Type != TypeCode.Null)
                return source.Substring(startIndex, length);
            else
                return "null";
        }
        #endregion

        #region IComparable<JValue>
        public int CompareTo(JValue other)
        {
            var a = source ?? string.Empty;
            var b = other.source ?? string.Empty;
            return string.Compare(a, startIndex, b, other.startIndex, Math.Max(length, other.length), CultureInfo.InvariantCulture, CompareOptions.Ordinal);
        }
        #endregion

        #region IEquatable<JValue>
        public bool Equals(JValue other)
        {
            return CompareTo(other) == 0;
        }
        #endregion
        #endregion

        #region Implicit Conversion
        public static implicit operator bool(JValue value)
        {
            return value.AsBoolean();
        }

        public static implicit operator int(JValue value)
        {
            return value.AsInt();
        }

        public static implicit operator long(JValue value)
        {
            return value.AsLong();
        }

        public static implicit operator float(JValue value)
        {
            return value.AsFloat();
        }

        public static implicit operator double(JValue value)
        {
            return value.AsDouble();
        }

        public static implicit operator string(JValue value)
        {
            return value.AsString();
        }

        public static implicit operator JValue(bool value)
        {
            return new JValue(value);
        }

        public static implicit operator JValue(int value)
        {
            return new JValue(value);
        }

        public static implicit operator JValue(long value)
        {
            return new JValue(value);
        }

        public static implicit operator JValue(float value)
        {
            return new JValue(value);
        }

        public static implicit operator JValue(double value)
        {
            return new JValue(value);
        }

        public static implicit operator JValue(string value)
        {
            return new JValue(value);
        }
        #endregion

        #region Operators
        public static bool operator ==(JValue left, JValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JValue left, JValue right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(JValue left, JValue right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(JValue left, JValue right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(JValue left, JValue right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(JValue left, JValue right)
        {
            return left.CompareTo(right) >= 0;
        }
        #endregion
    }

    #region Extensions
    /// <summary>
    /// Support utility class for JValue
    /// !No Heap Memory Allocation!
    /// </summary>
    public static class JValueExtensions
    {
        public static int Parse(string s, int startIndex, int length, int defaultValue)
        {
            var i = startIndex;
            if (s[startIndex] == '-' || s[startIndex] == '+')
                i++;

            var result = 0;
            length += startIndex;
            for (; i < length; i++)
            {
                if ('0' <= s[i] && s[i] <= '9')
                {
                    result = (result * 10) + (s[i] - '0');
                    if (result < 0)  // is overflow
                        return defaultValue;
                }
                else
                    return defaultValue;
            }

            if (s[startIndex] == '-')
                result = -result;

            return result;
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
    #endregion
}