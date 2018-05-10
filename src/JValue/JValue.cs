using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Halak
{
    /// <summary>
    /// Super lightweight JSON Reader
    /// </summary>
    /// <seealso cref="http://www.json.org/"/>
    /// <seealso cref="https://github.com/halak/jvalue/"/>
    public partial struct JValue : IComparable<JValue>, IEquatable<JValue>
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
        public static readonly JValue True = new JValue(true);
        public static readonly JValue False = new JValue(false);
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
        /// <seealso cref="JValue.Get(int)"/>
        public JValue this[int index] { get { return Get(index); } }
        /// <seealso cref="JValue.Get(string)"/>
        public JValue this[string key] { get { return Get(key); } }
        #endregion

        #region Constructors
        public static JValue Parse(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var index = SkipWhitespaces(source);
            return new JValue(source, index, source.Length - index);
        }

        public JValue(bool value) : this(value ? JsonHelper.TrueString : JsonHelper.FalseString, false) { }
        public JValue(int value) : this(value.ToString(CultureInfo.InvariantCulture), false) { }
        public JValue(long value) : this(value.ToString(CultureInfo.InvariantCulture), false) { }
        public JValue(ulong value) : this(value.ToString(CultureInfo.InvariantCulture), false) { }
        public JValue(float value) : this(value.ToString(CultureInfo.InvariantCulture), false) { }
        public JValue(double value) : this(value.ToString(CultureInfo.InvariantCulture), false) { }
        public JValue(decimal value) : this(value.ToString(CultureInfo.InvariantCulture), false) { }
        public JValue(string value)
        {
            if (value != null)
            {
                var builder = new StringBuilder(value.Length + 2);
                JsonHelper.AppendEscapedString(builder, value);

                source = builder.ToString();
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

        private JValue(string source, bool payload)
        {
            this.source = source;
            this.startIndex = 0;
            this.length = source.Length;
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
        public bool AsBoolean(bool defaultValue = false)
        {
            switch (Type)
            {
                case TypeCode.Null:
                    return defaultValue;
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
                    return defaultValue;
            }
        }

        private bool AsBooleanActually()
        {
            return source[startIndex] == 't';
        }

        public int AsInt32(int defaultValue = 0)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1 : 0;
                case TypeCode.Number:
                    return AsInt32Actually(defaultValue);
                case TypeCode.String:
                    return new JValue(source, startIndex + 1, length - 2).AsInt32Actually(defaultValue);  // TODO: escaped string issue
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

        private int AsInt32Actually(int defaultValue = 0)
        {
            if (IsInteger())
                return JsonHelper.Parse(source, startIndex, length, defaultValue);
            else
                return (int)JsonHelper.Parse(source, startIndex, length, (double)defaultValue);
        }

        public long AsInt64(long defaultValue = 0)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1 : 0;
                case TypeCode.Number:
                    return AsInt64Actually(defaultValue);
                case TypeCode.String:
                    return new JValue(source, startIndex + 1, length - 2).AsInt64Actually(defaultValue);  // TODO: escaped string issue
                default:
                    return defaultValue;
            }
        }

        private long AsInt64Actually(long defaultValue = 0L)
        {
            if (IsInteger())
                return JsonHelper.Parse(source, startIndex, length, defaultValue);
            else
                return (long)JsonHelper.Parse(source, startIndex, length, (double)defaultValue);
        }

        public float AsSingle(float defaultValue = 0.0f)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1 : 0;
                case TypeCode.Number:
                    return AsSingleActually(defaultValue);
                case TypeCode.String:
                    return new JValue(source, startIndex + 1, length - 2).AsSingleActually(defaultValue);  // TODO: escaped string issue
                default:
                    return defaultValue;
            }
        }

        private float AsSingleActually(float defaultValue = 0.0f)
        {
            return JsonHelper.Parse(source, startIndex, length, defaultValue);
        }

        public double AsDouble(double defaultValue = 0.0)
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? 1.0 : 0.0;
                case TypeCode.Number:
                    return AsDoubleActually(defaultValue);
                case TypeCode.String:
                    return new JValue(source, startIndex + 1, length - 2).AsDoubleActually(defaultValue);  // TODO: escaped string issue
                default:
                    return defaultValue;
            }
        }

        private double AsDoubleActually(double defaultValue = 0.0)
        {
            return JsonHelper.Parse(source, startIndex, length, defaultValue);
        }

        public string AsString(string defaultValue = "")
        {
            switch (Type)
            {
                case TypeCode.Boolean:
                    return AsBooleanActually() ? JsonHelper.TrueString : JsonHelper.FalseString;
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
            var sb = new StringBuilder(length);
            var enumerator = GetCharEnumerator();
            while (enumerator.MoveNext())
                sb.Append(enumerator.Current);

            return sb.ToString();
        }

        public List<JValue> AsArray()
        {
            var result = new List<JValue>(GetElementCount());
            foreach (var item in Array())
                result.Add(item);

            return result;
        }

        public Dictionary<JValue, JValue> AsObject()
        {
            var result = new Dictionary<JValue, JValue>(GetElementCount());
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
        /// Trace.Assert(x[1].AsInt32() == 2);
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
            var count = 0;
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
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        if (count == 0)
                            count = 1;
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

                    if (EqualsKey(key, source, kStart, kEnd - kStart - 2))
                        return new JValue(source, vStart, vEnd - vStart);

                    kStart = SkipWhitespaces(vEnd + 1);
                }
            }

            return JValue.Null;
        }

        private static bool EqualsKey(string a, string b, int bStart, int bLength)
        {
            if (a.Length > bLength)
                return false;

            var aIndex = 0;
            var bEnumerator = new CharEnumerator(b, bStart);
            for (; ; )
            {
                var x = aIndex < a.Length;
                var y = bEnumerator.MoveNext();
                if (x && y)
                {
                    if (a[aIndex++] != bEnumerator.Current)
                        return false;
                }
                else
                    return x == y;
            }
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

        public IEnumerable<KeyValuePair<JValue, JValue>> Object()
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

                    yield return new KeyValuePair<JValue, JValue>(new JValue(source, kStart, kEnd - kStart),
                                                                  new JValue(source, vStart, vEnd - vStart));
                    kStart = SkipWhitespaces(vEnd + 1);
                }
            }
        }

        public CharEnumerator GetCharEnumerator() => new CharEnumerator(this);

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

        private int SkipWhitespaces(int index) { return SkipWhitespaces(source, index, startIndex + length); }
        private static int SkipWhitespaces(string source) { return SkipWhitespaces(source, 0, source.Length); }
        private static int SkipWhitespaces(string source, int index, int end)
        {
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
                    case '/':
                        if (index + 1 < end)
                        {
                            if (source[index + 1] == '/')
                                index = SkipSinglelineComment(source, index + 2, end);
                            else if (source[index + 1] == '*')
                                index = SkipMultilineComment(source, index + 2, end);
                            else
                                return index;
                        }
                        break;
                    default:
                        return index;
                }
            }

            return end;
        }

        private static int SkipSinglelineComment(string source, int index, int end)
        {
            for (; index < end; index++)
            {
                switch (source[index])
                {
                    case '\r':
                    case '\n':
                        return index;
                }
            }

            return end;
        }

        private static int SkipMultilineComment(string source, int index, int end)
        {
            for (; index < end; index++)
            {
                if (source[index] == '*')
                {
                    if (index + 1 < end && source[index + 1] == '/')
                        return index + 2;
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
        public string Serialize(int indent = 2)
        {
            var builder = new StringBuilder(indent == 0 ? length : length * 2);
            Serialize(builder, indent);
            return builder.ToString();
        }

        public void Serialize(StringBuilder builder, int indent = 2)
        {
            Serialize(builder, this, indent, 0);
        }

        private static void Indent(StringBuilder builder, int indent, int depth)
        {
            var spaces = indent * depth;
            for (var i = 0; i < spaces; i++)
                builder.Append(' ');
        }

        private static void Serialize(StringBuilder builder, JValue value, int indent, int depth)
        {
            switch (value.Type)
            {
                case TypeCode.Array:
                    Serialize(builder, value.Array(), indent, depth, indent > 0 && value.length > 80);
                    break;
                case TypeCode.Object:
                    Serialize(builder, value.Object(), indent, depth, indent > 0 && value.length > 80);
                    break;
                default:
                    builder.Append(value.ToString());
                    break;
            }
        }

        private static void Serialize(StringBuilder builder, IEnumerable<JValue> value, int indent, int depth, bool multiline)
        {
            builder.Append('[');

            if (indent > 0 && multiline)
                builder.AppendLine();

            var isFirst = true;
            foreach (var item in value)
            {
                if (isFirst == false)
                {
                    builder.Append(',');

                    if (indent > 0)
                    {
                        if (multiline)
                            builder.AppendLine();
                        else
                            builder.Append(' ');
                    }
                }
                else
                    isFirst = false;

                if (indent > 0 && multiline)
                    Indent(builder, indent, depth + 1);

                Serialize(builder, item, indent, depth + 1);
            }

            if (indent > 0 && multiline)
            {
                builder.AppendLine();
                Indent(builder, indent, depth);
            }

            builder.Append(']');
        }

        private static void Serialize(StringBuilder builder, IEnumerable<KeyValuePair<JValue, JValue>> value, int indent, int depth, bool multiline)
        {
            builder.Append('{');

            if (indent > 0 && multiline)
                builder.AppendLine();

            var isFirst = true;
            foreach (var item in value)
            {
                if (isFirst == false)
                {
                    builder.Append(',');

                    if (indent > 0)
                    {
                        if (multiline)
                            builder.AppendLine();
                        else
                            builder.Append(' ');
                    }
                }
                else
                    isFirst = false;

                if (indent > 0 && multiline)
                    Indent(builder, indent, depth + 1);

                Serialize(builder, item.Key, indent, depth + 1);
                builder.Append(':');
                if (indent > 0)
                    builder.Append(' ');
                Serialize(builder, item.Value, indent, depth + 1);
            }

            if (indent > 0 && multiline)
            {
                builder.AppendLine();
                Indent(builder, indent, depth);
            }

            builder.Append('}');
        }
        #endregion

        #region System.Object
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
                return (startIndex == 0 && length == source.Length) ? source : source.Substring(startIndex, length);
            else
                return JsonHelper.NullString;
        }
        #endregion

        #region System.IComparable<JValue>
        public int CompareTo(JValue other)
        {
            if (Equals(other))
                return 0;
            else
                return string.CompareOrdinal(
                    source ?? string.Empty, startIndex,
                    other.source ?? string.Empty, other.startIndex,
                    Math.Max(length, other.length));
        }
        #endregion

        #region System.IEquatable<JValue>
        public bool Equals(JValue other)
        {
            var t = Type;
            if (t == other.Type)
            {
                switch (t)
                {
                    case TypeCode.Null:
                        return true;
                    case TypeCode.Boolean:
                        return AsBooleanActually() == other.AsBooleanActually();
                    case TypeCode.Number:
                        return AsDoubleActually() == other.AsDoubleActually();
                    case TypeCode.String:
                        return EqualsString(other);
                    default:
                        return startIndex == other.startIndex &&
                            length == other.length &&
                            ReferenceEquals(source, other.source);
                }
            }
            else
                return false;
        }

        private bool EqualsString(JValue other)
        {
            var aEnd = startIndex + length - 1;
            var bEnd = other.startIndex + other.length - 1;

            var aEnumerator = GetCharEnumerator();
            var bEnumerator = other.GetCharEnumerator();

            for(; ;)
            {
                var a = aEnumerator.MoveNext();
                var b = bEnumerator.MoveNext();
                if (a && b)
                {
                    if (aEnumerator.Current != bEnumerator.Current)
                        return false;
                }
                else
                    return a == b;
            }
        }
        #endregion
        #endregion

        #region Implicit Conversion
        public static implicit operator bool(JValue value) { return value.AsBoolean(); }
        public static implicit operator int(JValue value) { return value.AsInt32(); }
        public static implicit operator long(JValue value) { return value.AsInt64(); }
        public static implicit operator float(JValue value) { return value.AsSingle(); }
        public static implicit operator double(JValue value) { return value.AsDouble(); }
        public static implicit operator string(JValue value) { return value.AsString(); }
        public static implicit operator JValue(bool value) { return new JValue(value); }
        public static implicit operator JValue(int value) { return new JValue(value); }
        public static implicit operator JValue(long value) { return new JValue(value); }
        public static implicit operator JValue(float value) { return new JValue(value); }
        public static implicit operator JValue(double value) { return new JValue(value); }
        public static implicit operator JValue(string value) { return new JValue(value); }
        #endregion

        #region Operators
        public static bool operator ==(JValue left, JValue right) { return left.Equals(right); }
        public static bool operator !=(JValue left, JValue right) { return !left.Equals(right); }
        public static bool operator <(JValue left, JValue right) { return left.CompareTo(right) < 0; }
        public static bool operator <=(JValue left, JValue right) { return left.CompareTo(right) <= 0; }
        public static bool operator >(JValue left, JValue right) { return left.CompareTo(right) > 0; }
        public static bool operator >=(JValue left, JValue right) { return left.CompareTo(right) >= 0; }
        #endregion

        public struct CharEnumerator : IEnumerator<char>
        {
            private readonly string source;
            private readonly int startIndex;
            private char current;
            private int index;

            public char Current => current;
            object IEnumerator.Current => Current;

            internal CharEnumerator(JValue value) : this(value.source, value.startIndex) { }
            internal CharEnumerator(string source, int startIndex)
            {
                this.source = source;
                this.startIndex = startIndex;
                this.current = '\0';
                this.index = startIndex;
            }

            public bool MoveNext()
            {
                if (index >= 0)
                {
                    current = source[++index];

                    if (current != '\\')
                    {
                        if (current != '"')
                            return true;
                        else
                        {
                            index = -1;
                            return false;
                        }
                    }
                    else
                    {
                        index++;

                        switch (source[index])
                        {
                            case '"': current = '"'; break;
                            case '/': current = '/'; break;
                            case '\\': current = '\\'; break;
                            case 'n': current = '\n'; break;
                            case 't': current = '\t'; break;
                            case 'r': current = '\r'; break;
                            case 'b': current = '\n'; break;
                            case 'f': current = '\f'; break;
                            case 'u':
                                var a = source[++index];
                                var b = source[++index];
                                var c = source[++index];
                                var d = source[++index];
                                current = (char)((Hex(a) * 4096) + (Hex(b) * 256) + (Hex(c) * 16) + (Hex(d)));
                                break;
                        }

                        return true;
                    }
                }
                else
                    return false;
            }

            public void Reset()
            {
                current = '\0';
                index = startIndex;
            }

            public void Dispose() { }

            // FORCE-INLINE
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
}