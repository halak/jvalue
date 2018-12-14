using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
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
        public static readonly JValue Null = new JValue("null", false);
        public static readonly JValue True = new JValue(true);
        public static readonly JValue False = new JValue(false);
        public static readonly JValue EmptyString = new JValue("\"\"", false);
        public static readonly JValue EmptyArray = new JValue("[]", false);
        public static readonly JValue EmptyObject = new JValue("{}", false);
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
        public JValue this[int index] { get { return Get(index); } }
        public JValue this[string key] { get { return Get(key); } }
        #endregion

        #region Constructors
        public static JValue Parse(string source)
        {
            if (source != null)
            {
                var index = SkipWhitespaces(source);
                var end = BackwardSkipWhitespaces(source, source.Length - 1) + 1;
                return new JValue(source, index, end - index);
            }
            else
                return JValue.Null;
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

        public JValue(IEnumerable<JValue> array) : this(From(array)) { }
        public JValue(IEnumerable<KeyValuePair<string, JValue>> obj) : this(From(obj)) { }
        internal JValue(string source, int startIndex, int length)
        {
            this.source = source;
            this.startIndex = startIndex;
            this.length = length;
        }

        private JValue(string source, bool payload) : this(source, 0, source.Length) { }
        private JValue(JValue original) : this(original.source, original.startIndex, original.length) { }

        private static JValue From(IEnumerable<JValue> array)
        {
            var builder = new ArrayBuilder();
            foreach (var element in array)
                builder.Push(element);
            return builder.Build();
        }

        private static JValue From(IEnumerable<KeyValuePair<string, JValue>> obj)
        {
            var builder = new ObjectBuilder();
            foreach (var member in obj)
                builder.Put(member.Key, member.Value);
            return builder.Build();
        }
        #endregion

        #region Methods
        #region Convert
        public bool ToBoolean(bool defaultValue = false)
        {
            switch (Type)
            {
                case TypeCode.Null: return defaultValue;
                case TypeCode.Boolean: return ToBooleanActually();
                case TypeCode.Number: return ToDoubleActually(0.0) != 0.0;
                case TypeCode.String: return length != 2;  // two quotation marks
                case TypeCode.Array: return true;
                case TypeCode.Object: return true;
                default: return defaultValue;
            }
        }

        public int ToInt32(int defaultValue = 0)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanActually() ? 1 : 0;
                case TypeCode.Number: return ToInt32Actually(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToInt32Actually(defaultValue);
                default: return defaultValue;
            }
        }

        public long ToInt64(long defaultValue = 0)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanActually() ? 1 : 0;
                case TypeCode.Number: return ToInt64Actually(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToInt64Actually(defaultValue);
                default: return defaultValue;
            }
        }

        public float ToSingle(float defaultValue = 0.0f)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanActually() ? 1 : 0;
                case TypeCode.Number: return ToSingleActually(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToSingleActually(defaultValue);
                default: return defaultValue;
            }
        }

        public double ToDouble(double defaultValue = 0.0)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanActually() ? 1.0 : 0.0;
                case TypeCode.Number: return ToDoubleActually(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToDoubleActually(defaultValue);
                default: return defaultValue;
            }
        }

        public decimal ToDecimal(decimal defaultValue = 0.0m)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanActually() ? 1.0m : 0.0m;
                case TypeCode.Number: return ToDecimalActually(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToDecimalActually(defaultValue);
                default: return defaultValue;
            }
        }

        public JNumber ToNumber() { return ToNumber(JNumber.Zero); }
        public JNumber ToNumber(JNumber defaultValue)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanActually() ? JNumber.One : JNumber.Zero;
                case TypeCode.Number: return ToNumberActually(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToNumberActually(defaultValue);
                default: return JNumber.NaN;
            }
        }

        private bool ToBooleanActually() { return source[startIndex] == 't'; }
        private int ToInt32Actually(int defaultValue) { return JsonHelper.ParseInt32(source, startIndex, defaultValue); }
        private long ToInt64Actually(long defaultValue) { return JsonHelper.ParseInt64(source, startIndex, defaultValue); }
        private float ToSingleActually(float defaultValue) { return JsonHelper.ParseSingle(source, startIndex, defaultValue); }
        private double ToDoubleActually(double defaultValue) { return JsonHelper.ParseDouble(source, startIndex, defaultValue); }
        private decimal ToDecimalActually(decimal defaultValue) { return JsonHelper.ParseDecimal(source, startIndex, length, defaultValue); }
        private JNumber ToNumberActually(JNumber defaultValue)
        {
            var value = JNumber.NaN;
            if (JNumber.TryParse(source, startIndex, out value))
                return value;
            else
                return defaultValue;
        }

        public string ToUnescapedString(string defaultValue = "")
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanActually() ? JsonHelper.TrueString : JsonHelper.FalseString;
                case TypeCode.Number: return source.Substring(startIndex, length);
                case TypeCode.String: return ToUnescapedStringActually();
                default: return defaultValue;
            }
        }

        private string ToUnescapedStringActually()
        {
            var sb = new StringBuilder(length);
            var enumerator = GetCharEnumerator();
            while (enumerator.MoveNext())
                sb.Append(enumerator.Current);
            return sb.ToString();
        }

        private JValue ConvertForNumberParsing()
        {
            var end = startIndex + length - 1;
            for (var i = startIndex + 1; i < end; i++)
            {
                if (source[i] == '\\')
                    return new JValue(ToUnescapedStringActually(), false);
            }

            return new JValue(source, startIndex + 1, length - 2);
        }

        public List<JValue> ToArray()
        {
            var result = new List<JValue>();
            foreach (var item in Array())
                result.Add(item);

            return result;
        }

        public Dictionary<JValue, JValue> ToObject()
        {
            var result = new Dictionary<JValue, JValue>();
            foreach (var item in Object())
                result[item.Key] = item.Value;

            return result;
        }
        #endregion

        #region Get
        private JValue Get(int index)
        {
            if (Type == TypeCode.Array)
            {
                foreach (var item in Array())
                {
                    if (index-- == 0)
                        return item;
                }
            }

            return Null;
        }

        private JValue Get(string key)
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

        private static bool EqualsKey(string key, string escapedKey, int escapedKeyStart, int escapedKeyLength)
        {
            if (key.Length > escapedKeyLength)
                return false;

            var aIndex = 0;
            var bEnumerator = new CharEnumerator(escapedKey, escapedKeyStart);
            for (; ; )
            {
                var x = aIndex < key.Length;
                var y = bEnumerator.MoveNext();
                if (x && y)
                {
                    if (key[aIndex++] != bEnumerator.Current)
                        return false;
                }
                else
                    return x == y;
            }
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
                    default:
                        return index;
                }
            }

            return end;
        }
        private static int BackwardSkipWhitespaces(string source, int index)
        {
            for (; index >= 0; index--)
            {
                switch (source[index])
                {
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        return index;
                }
            }

            return -1;
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
        public override int GetHashCode() { return source.GetHashCode() + startIndex; }
        public override bool Equals(object obj) { return (obj is JValue) && Equals((JValue)obj); }

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
                        return ToBooleanActually() == other.ToBooleanActually();
                    case TypeCode.Number:
                        return ToDoubleActually(0.0) == other.ToDoubleActually(0.0);
                    case TypeCode.String:
                        return EqualString(this, other);
                    default:
                        return startIndex == other.startIndex &&
                            length == other.length &&
                            ReferenceEquals(source, other.source);
                }
            }
            else
                return false;
        }

        private static bool EqualString(JValue a, JValue b)
        {
            var aEnd = a.startIndex + a.length - 1;
            var bEnd = b.startIndex + b.length - 1;

            var aEnumerator = a.GetCharEnumerator();
            var bEnumerator = b.GetCharEnumerator();

            for (; ; )
            {
                var aStep = aEnumerator.MoveNext();
                var bStep = bEnumerator.MoveNext();
                if (aStep && bStep)
                {
                    if (aEnumerator.Current != bEnumerator.Current)
                        return false;
                }
                else
                    return aStep == bStep;
            }
        }
        #endregion
        #endregion

        #region Implicit Conversion
        public static implicit operator bool(JValue value) { return value.ToBoolean(); }
        public static implicit operator int(JValue value) { return value.ToInt32(); }
        public static implicit operator long(JValue value) { return value.ToInt64(); }
        public static implicit operator float(JValue value) { return value.ToSingle(); }
        public static implicit operator double(JValue value) { return value.ToDouble(); }
        public static implicit operator decimal(JValue value) { return value.ToDecimal(); }
        public static implicit operator string(JValue value) { return value.ToUnescapedString(); }
        public static implicit operator JValue(bool value) { return new JValue(value); }
        public static implicit operator JValue(int value) { return new JValue(value); }
        public static implicit operator JValue(long value) { return new JValue(value); }
        public static implicit operator JValue(float value) { return new JValue(value); }
        public static implicit operator JValue(double value) { return new JValue(value); }
        public static implicit operator JValue(decimal value) { return new JValue(value); }
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
                if (0 <= index && index < source.Length - 1)
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
                            case 'b': current = '\b'; break;
                            case 'f': current = '\f'; break;
                            case 'u':
                                var a = source[++index];
                                var b = source[++index];
                                var c = source[++index];
                                var d = source[++index];
                                current = (char)((Hex(a) << 12) | (Hex(b) << 8) | (Hex(c) << 4) | (Hex(d)));
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

#if !NET35
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
