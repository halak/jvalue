using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        internal const string NullLiteral = "null";
        internal const string TrueLiteral = "true";
        internal const string FalseLiteral = "false";

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
        public JValue this[int index] => Get(index);
        public JValue this[string key] => Get(key);
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
                return Null;
        }

        public JValue(bool value) : this(value ? TrueLiteral : FalseLiteral, false) { }
        public JValue(int value) : this(value.ToString(NumberFormatInfo.InvariantInfo), false) { }
        public JValue(long value) : this(value.ToString(NumberFormatInfo.InvariantInfo), false) { }
        public JValue(ulong value) : this(value.ToString(NumberFormatInfo.InvariantInfo), false) { }
        public JValue(float value) : this(value.ToString(NumberFormatInfo.InvariantInfo), false) { }
        public JValue(double value) : this(value.ToString(NumberFormatInfo.InvariantInfo), false) { }
        public JValue(decimal value) : this(value.ToString(NumberFormatInfo.InvariantInfo), false) { }
        public JValue(string value)
        {
            if (value != null)
            {
                using (var writer = new StringWriter(new StringBuilder(value.Length + 2), CultureInfo.InvariantCulture))
                {
                    writer.WriteEscapedString(value);
                    source = writer.GetStringBuilder().ToString();
                    startIndex = 0;
                    length = source.Length;
                }
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

        private JValue(string source, bool _) : this(source, 0, source.Length) { }
        private JValue(JValue original) : this(original.source, original.startIndex, original.length) { }

        private static JValue From(IEnumerable<JValue> array)
        {
            var builder = new JsonArrayBuilder(512);
            foreach (var element in array)
                builder.Push(element);
            return builder.Build();
        }

        private static JValue From(IEnumerable<KeyValuePair<string, JValue>> obj)
        {
            var builder = new JsonObjectBuilder(512);
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
                case TypeCode.Boolean: return ToBooleanCore();
                case TypeCode.Number: return ToDoubleCore(0.0) != 0.0;
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
                case TypeCode.Boolean: return ToBooleanCore() ? 1 : 0;
                case TypeCode.Number: return ToInt32Core(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToInt32Core(defaultValue);
                default: return defaultValue;
            }
        }

        public long ToInt64(long defaultValue = 0)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanCore() ? 1 : 0;
                case TypeCode.Number: return ToInt64Core(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToInt64Core(defaultValue);
                default: return defaultValue;
            }
        }

        public float ToSingle(float defaultValue = 0.0f)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanCore() ? 1 : 0;
                case TypeCode.Number: return ToSingleCore(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToSingleCore(defaultValue);
                default: return defaultValue;
            }
        }

        public double ToDouble(double defaultValue = 0.0)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanCore() ? 1.0 : 0.0;
                case TypeCode.Number: return ToDoubleCore(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToDoubleCore(defaultValue);
                default: return defaultValue;
            }
        }

        public decimal ToDecimal(decimal defaultValue = 0.0m)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanCore() ? 1.0m : 0.0m;
                case TypeCode.Number: return ToDecimalCore(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToDecimalCore(defaultValue);
                default: return defaultValue;
            }
        }

        public JNumber ToNumber() { return ToNumber(JNumber.Zero); }
        public JNumber ToNumber(JNumber defaultValue)
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanCore() ? JNumber.One : JNumber.Zero;
                case TypeCode.Number: return ToNumberCore(defaultValue);
                case TypeCode.String: return ConvertForNumberParsing().ToNumberCore(defaultValue);
                default: return JNumber.NaN;
            }
        }

        private bool ToBooleanCore()
            => source[startIndex] == 't';

        private int ToInt32Core(int defaultValue)
            => JNumber.ParseInt32(source, startIndex, defaultValue);
        private long ToInt64Core(long defaultValue)
            => JNumber.ParseInt64(source, startIndex, defaultValue);
        private float ToSingleCore(float defaultValue)
            => JNumber.ParseSingle(source, startIndex, defaultValue);
        private double ToDoubleCore(double defaultValue)
            => JNumber.ParseDouble(source, startIndex, defaultValue);
        private decimal ToDecimalCore(decimal defaultValue)
            => JNumber.ParseDecimal(source, startIndex, length, defaultValue);
        private JNumber ToNumberCore(JNumber defaultValue)
            => JNumber.TryParse(source, startIndex, out var value) ? value : defaultValue;

        public string ToUnescapedString(string defaultValue = "")
        {
            switch (Type)
            {
                case TypeCode.Boolean: return ToBooleanCore() ? TrueLiteral : FalseLiteral;
                case TypeCode.Number: return source.Substring(startIndex, length);
                case TypeCode.String: return ToUnescapedStringCore();
                default: return defaultValue;
            }
        }

        private string ToUnescapedStringCore()
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
                    return new JValue(ToUnescapedStringCore(), false);
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
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
                Serialize(writer, this, indent, 0);
        }

        public void Serialize(TextWriter writer, int indent = 2)
            => Serialize(writer, this, indent, 0);

        internal void WriteTo(TextWriter writer)
        {
            if (Type != TypeCode.Null)
            {
                var end = startIndex + length;
                for (var i = startIndex; i < end; i++)
                    writer.Write(source[i]);
            }
            else
                writer.Write(NullLiteral);
        }

        private static void Indent(TextWriter writer, int indent, int depth)
        {
            var spaces = indent * depth;
            for (var i = 0; i < spaces; i++)
                writer.Write(' ');
        }

        private static void Serialize(TextWriter writer, JValue value, int indent, int depth)
        {
            switch (value.Type)
            {
                case TypeCode.Array:
                    Serialize(writer, value.Array(), indent, depth, indent > 0 && value.length > 80);
                    break;
                case TypeCode.Object:
                    Serialize(writer, value.Object(), indent, depth, indent > 0 && value.length > 80);
                    break;
                default:
                    value.WriteTo(writer);
                    break;
            }
        }

        private static void Serialize(TextWriter writer, IEnumerable<JValue> value, int indent, int depth, bool multiline)
        {
            writer.Write('[');

            if (indent > 0 && multiline)
                writer.WriteLine();

            var isFirst = true;
            foreach (var item in value)
            {
                if (isFirst == false)
                {
                    writer.Write(',');

                    if (indent > 0)
                    {
                        if (multiline)
                            writer.WriteLine();
                        else
                            writer.Write(' ');
                    }
                }
                else
                    isFirst = false;

                if (indent > 0 && multiline)
                    Indent(writer, indent, depth + 1);

                Serialize(writer, item, indent, depth + 1);
            }

            if (indent > 0 && multiline)
            {
                writer.WriteLine();
                Indent(writer, indent, depth);
            }

            writer.Write(']');
        }

        private static void Serialize(TextWriter writer, IEnumerable<KeyValuePair<JValue, JValue>> value, int indent, int depth, bool multiline)
        {
            writer.Write('{');

            if (indent > 0 && multiline)
                writer.WriteLine();

            var isFirst = true;
            foreach (var item in value)
            {
                if (isFirst == false)
                {
                    writer.Write(',');

                    if (indent > 0)
                    {
                        if (multiline)
                            writer.WriteLine();
                        else
                            writer.Write(' ');
                    }
                }
                else
                    isFirst = false;

                if (indent > 0 && multiline)
                    Indent(writer, indent, depth + 1);

                Serialize(writer, item.Key, indent, depth + 1);
                writer.Write(':');
                if (indent > 0)
                    writer.Write(' ');
                Serialize(writer, item.Value, indent, depth + 1);
            }

            if (indent > 0 && multiline)
            {
                writer.WriteLine();
                Indent(writer, indent, depth);
            }

            writer.Write('}');
        }
        #endregion

        public override int GetHashCode()
        {
            switch (Type)
            {
                case TypeCode.Null: return 0;
                case TypeCode.Boolean: return ToBoolean() ? 0x392307A6 : 0x63D95114;
                case TypeCode.Number: return ToNumber().GetHashCode();
                case TypeCode.String: return GetStringHashCode();
                case TypeCode.Array: return GetArrayHashCode();
                case TypeCode.Object: return GetObjectHashCode();
                default: return 0;
            }
        }

        public bool Equals(JValue other) => Equals(this, other);
        public int CompareTo(JValue other) => Compare(this, other);
        public override bool Equals(object obj) => obj is JValue other && Equals(other);
        public override string ToString()
        {
            if (Type != TypeCode.Null)
                return (startIndex == 0 && length == source.Length) ? source : source.Substring(startIndex, length);
            else
                return NullLiteral;
        }

        #region HashCode
        private int GetStringHashCode()
        {
            var enumerator = GetCharEnumerator();
            var hashCode = 0x219FFA9C;
            while (enumerator.MoveNext())
                hashCode = HashCode.Combine(hashCode, enumerator.Current);
            return hashCode;
        }

        private int GetArrayHashCode()
        {
            var hashCode = 0x12D398BA;
            foreach (var element in Array())
                hashCode = HashCode.Combine(hashCode, element.GetHashCode());
            return hashCode;
        }

        private int GetObjectHashCode()
        {
            var hashCode = 0x50638734;
            foreach (var member in Object())
            {
                hashCode = HashCode.Combine(hashCode, member.Key.GetHashCode());
                hashCode = HashCode.Combine(hashCode, member.Value.GetHashCode());
            }
            return hashCode;
        }
        #endregion

        public static bool Equals(JValue left, JValue right)
        {
            var leftType = left.Type;
            var rightType = right.Type;
            if (leftType == rightType)
            {
                switch (leftType)
                {
                    case TypeCode.Null: return true;
                    case TypeCode.Boolean: return left.ToBooleanCore() == right.ToBooleanCore();
                    case TypeCode.Number: return JNumber.Equals(left.ToNumberCore(JNumber.NaN), right.ToNumberCore(JNumber.NaN));
                    case TypeCode.String: return EqualsString(left, right);
                    case TypeCode.Array: return SequenceEqual(left.Array().GetEnumerator(), right.Array().GetEnumerator(), Equals);
                    case TypeCode.Object: return SequenceEqual(left.Object().GetEnumerator(), right.Object().GetEnumerator(), EqualsMember);
                }
            }

            return false;
        }

        public static int Compare(JValue left, JValue right)
        {
            var leftType = left.Type;
            var rightType = right.Type;
            if (leftType == rightType)
            {
                switch (leftType)
                {
                    case TypeCode.Null: return 0;
                    case TypeCode.Boolean: return left.ToBooleanCore().CompareTo(right.ToBooleanCore());
                    case TypeCode.Number: return JNumber.Compare(left.ToNumberCore(JNumber.NaN), right.ToNumberCore(JNumber.NaN));
                    case TypeCode.String: return SequenceCompare<char, CharEnumerator>(left.GetCharEnumerator(), right.GetCharEnumerator(), (x, y) => x.CompareTo(y));
                    case TypeCode.Array: return SequenceCompare(left.Array().GetEnumerator(), right.Array().GetEnumerator(), Compare);
                    case TypeCode.Object: return SequenceCompare(left.Object().GetEnumerator(), right.Object().GetEnumerator(), CompareMember);
                    default: return 0;
                }
            }
            else
                return ((int)leftType).CompareTo((int)rightType);
        }

        private static int CompareMember(KeyValuePair<JValue, JValue> x, KeyValuePair<JValue, JValue> y)
        {
            var k = Compare(x.Key, y.Key);
            if (k != 0)
                return k;
            else
                return Compare(x.Value, y.Value);
        }

        private static int SequenceCompare<T>(IEnumerator<T> a, IEnumerator<T> b, Func<T, T, int> compare)
            => SequenceCompare<T, IEnumerator<T>>(a, b, compare);
        private static int SequenceCompare<T, TEnumerator>(TEnumerator a, TEnumerator b, Func<T, T, int> compare) where TEnumerator : IEnumerator<T>
        {
            for (; ; )
            {
                var aStep = a.MoveNext();
                var bStep = b.MoveNext();
                if (aStep && bStep)
                {
                    var result = compare(a.Current, b.Current);
                    if (result != 0)
                        return result;
                }
                else
                    return 0;
            }
        }

        private static bool EqualsString(JValue a, JValue b)
            => SequenceEqual<char, CharEnumerator>(a.GetCharEnumerator(), b.GetCharEnumerator(), (x, y) => x == y);
        private static bool EqualsMember(KeyValuePair<JValue, JValue> x, KeyValuePair<JValue, JValue> y)
            => Equals(x.Key, y.Key) && Equals(x.Value, y.Value);
        private static bool SequenceEqual<T>(IEnumerator<T> a, IEnumerator<T> b, Func<T, T, bool> equals)
            => SequenceEqual<T, IEnumerator<T>>(a, b, equals);
        private static bool SequenceEqual<T, TEnumerator>(TEnumerator a, TEnumerator b, Func<T, T, bool> equals) where TEnumerator : IEnumerator<T>
        {
            for (; ; )
            {
                var aStep = a.MoveNext();
                var bStep = b.MoveNext();
                if (aStep && bStep)
                {
                    if (equals(a.Current, b.Current) == false)
                        return false;
                }
                else
                    return aStep == bStep;
            }
        }
        #endregion

        #region Implicit Conversion
        public static implicit operator bool(JValue value) => value.ToBoolean();
        public static implicit operator int(JValue value) => value.ToInt32();
        public static implicit operator long(JValue value) => value.ToInt64();
        public static implicit operator float(JValue value) => value.ToSingle();
        public static implicit operator double(JValue value) => value.ToDouble();
        public static implicit operator decimal(JValue value) => value.ToDecimal();
        public static implicit operator string(JValue value) => value.ToUnescapedString();
        public static implicit operator JValue(bool value) => new JValue(value);
        public static implicit operator JValue(int value) => new JValue(value);
        public static implicit operator JValue(long value) => new JValue(value);
        public static implicit operator JValue(float value) => new JValue(value);
        public static implicit operator JValue(double value) => new JValue(value);
        public static implicit operator JValue(decimal value) => new JValue(value);
        public static implicit operator JValue(string value) => new JValue(value);
        #endregion

        #region Operators
        public static bool operator ==(JValue left, JValue right) => left.Equals(right);
        public static bool operator !=(JValue left, JValue right) => left.Equals(right) == false;
        public static bool operator <(JValue left, JValue right) => left.CompareTo(right) < 0;
        public static bool operator <=(JValue left, JValue right) => left.CompareTo(right) <= 0;
        public static bool operator >(JValue left, JValue right) => left.CompareTo(right) > 0;
        public static bool operator >=(JValue left, JValue right) => left.CompareTo(right) >= 0;
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
