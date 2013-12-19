using System;
using System.Collections;
using System.Collections.Generic;

namespace Halak
{
    /// <summary>
    /// Super lightweight JSON Reader
    /// </summary>
    /// <seealso cref="http://www.json.org/"/>
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
                if (string.IsNullOrEmpty(source))
                    return TypeCode.Null;

                switch (source[startIndex])
                {
                    case 't':
                    case 'f':
                        return TypeCode.Boolean;
                    case '"':
                        return TypeCode.String;
                    case '[':
                        return TypeCode.Array;
                    case '{':
                        return TypeCode.Object;
                    default:
                        return TypeCode.Number;
                }
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
        public JValue(string source)
        {
            if (string.IsNullOrEmpty(source))
                source = string.Empty;

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
                    return length != 2; // two quotation marks
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
                    int value;
                    if (int.TryParse(AsStringActually(), out value))
                        return value;
                    else
                        return defaultValue;
                default:
                    return defaultValue;
            }
        }

        private bool IsInteger()
        {
            for (int i = startIndex; i < startIndex + length; i++)
            {
                char c = source[i];
                if (c == '.' || c == 'e' || c == 'E')
                    return false;
            }

            return true;
        }

        private int AsIntActually(int defaultValue = 0)
        {
            string s = source.Substring(startIndex, length);
            if (IsInteger())
            {
                int intValue;
                if (int.TryParse(s, out intValue))
                    return intValue;
                else
                    return defaultValue;
            }
            else
            {
                double doubleValue;
                if (double.TryParse(s, out doubleValue))
                    return (int)doubleValue;
                else
                    return defaultValue;
            }
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
                    long value;
                    if (long.TryParse(AsStringActually(), out value))
                        return value;
                    else
                        return defaultValue;
                default:
                    return defaultValue;
            }
        }

        private long AsLongActually(long defaultValue = 0)
        {
            string s = source.Substring(startIndex, length);
            if (IsInteger())
            {
                long longValue;
                if (long.TryParse(s, out longValue))
                    return longValue;
                else
                    return defaultValue;
            }
            else
            {
                double doubleValue;
                if (double.TryParse(s, out doubleValue))
                    return (long)doubleValue;
                else
                    return defaultValue;
            }
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
                    float value;
                    if (float.TryParse(AsStringActually(), out value))
                        return value;
                    else
                        return defaultValue;
                default:
                    return defaultValue;
            }
        }

        private float AsSingleActually(float defaultValue = 0.0f)
        {
            string s = source.Substring(startIndex, length);

            float floatValue;
            if (float.TryParse(s, out floatValue))
                return floatValue;
            else
                return defaultValue;
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
                    double value;
                    if (double.TryParse(AsStringActually(), out value))
                        return value;
                    else
                        return defaultValue;
                default:
                    return defaultValue;
            }
        }

        private double AsDoubleActually(double defaultValue = 0.0)
        {
            string s = source.Substring(startIndex, length);

            double doubleValue;
            if (double.TryParse(s, out doubleValue))
                return doubleValue;
            else
                return defaultValue;
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
            return source.Substring(startIndex + 1, length - 2);
        }

        public List<JValue> AsArray()
        {
            var result = new List<JValue>();
            foreach (var item in Array())
                result.Add(item);

            return result;
        }

        public Dictionary<string, JValue> AsObject()
        {
            var result = new Dictionary<string, JValue>();
            foreach (var item in Object())
                result[item.Key] = item.Value;

            return result;
        }
        #endregion

        #region Get
        /// <summary>
        /// �Է��� ���ο� �����ϴ� JValue�� �����ɴϴ�.
        /// ���� ������ ������ �Է��ϸ� �ڿ������� �����ɴϴ�.
        /// </summary>
        /// <param name="index">����</param>
        /// <returns>�Է��� ���ο� �����ϴ� JValue ��. ���� ��ü�� �迭�� �ƴϰų� ������ ������ ������� JValue.Null�� ��ȯ�մϴ�.</returns>
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
                    index += GetArrayLength();

                foreach (var item in Array())
                {
                    if (index-- == 0)
                        return item;
                }
            }

            return Null;
        }

        private int GetArrayLength()
        {
            int count = 0;
            foreach (var item in Array())
                count++;
            return count;
        }

        /// <summary>
        /// �Է��� �̸��� �ش��ϴ� ���� JValue�� �����ɴϴ�.
        /// ��(.)���� �̸��� �����Ͽ� ���� ��ü�� ���� ��ü���� Ž���Ͽ� ������ �� �ֽ��ϴ�.
        /// </summary>
        /// <param name="key">�̸�</param>
        /// <returns>�Է��� �̸��� �����ϴ� JValue ��. Ž���߿� ���� ��ü�� ã�� ���ϸ� Ž���� �ߴ��ϰ� JValue.Null�� ��ȯ�մϴ�.</returns>
        /// <example>
        /// <code>
        /// var x = new JValue("{hello:{world:10}}");
        /// Trace.Assert(x["hello.world"] == 10);
        /// Trace.Assert(x["hello"]["world"] == 10);
        /// </code>
        /// </example>
        public JValue Get(string key)
        {
            var current = this;
            foreach (var item in key.SplitAndEnumerate('.'))
                current = current.GetFromOneDepth(item);

            return current;
        }

        private JValue GetFromOneDepth(string key)
        {
            // TODO: OPTIMIZE

            if (Type == TypeCode.Object)
            {
                foreach (var item in Object())
                {
                    if (item.Key == key)
                        return item.Value;
                }
            }

            return JValue.Null;
        }
        #endregion

        #region Enumeration
        public IEnumerable<JValue> Array()
        {
            if (Type == TypeCode.Array)
            {
                int start = startIndex + 1;
                int end = NextComma(start);
                while (end != -1)
                {
                    yield return new JValue(source, start, end - start);
                    start = end + 1;
                    end = NextComma(start);
                }
            }
        }

        public IEnumerable<KeyValuePair<int, JValue>> IndexedArray()
        {
            if (Type == TypeCode.Array)
            {
                int index = 0;
                int start = startIndex + 1;
                int end = NextComma(start);
                while (end != -1)
                {
                    yield return new KeyValuePair<int, JValue>(index++, new JValue(source, start, end - start));
                    start = end + 1;
                    end = NextComma(start);
                }
            }
        }

        public IEnumerable<KeyValuePair<string, JValue>> Object()
        {
            if (Type == TypeCode.Object)
            {
                int keyStart = NextToKeyStart(startIndex + 1);
                while (keyStart != -1)
                {
                    int keyEnd = NextToKeyEnd(keyStart + 1);
                    int valueStart = NextToValueStart(keyEnd);
                    int valueEnd = NextComma(valueStart);
                    yield return new KeyValuePair<string, JValue>(source.Substring(keyStart, keyEnd - keyStart),
                                                                  new JValue(source, valueStart, valueEnd - valueStart));
                    keyStart = NextToKeyStart(valueEnd + 1);
                }
            }
        }

        // TODO: public IEnumerable<Tuple<int keyStart, int keyEnd, JValue>> Object()

        private int NextComma(int index)
        {
            int endIndex = startIndex + length;
            if (index >= endIndex)
                return -1;

            int depth = 0;
            bool inQuotes = false;
            for (; index < endIndex; index++)
            {
                if (inQuotes == false)
                {
                    switch (source[index])
                    {
                        case ',':
                            if (depth == 0)
                                return index;
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
                            inQuotes = true;
                            break;
                    }
                }
                else
                {
                    switch (source[index])
                    {
                        case '"':
                            inQuotes = false;
                            break;
                        case '\\':
                            index++;
                            break;
                    }
                }
            }

            return endIndex - 1;
        }

        private int NextToNoWhiteSpaces(int index, int endIndex)
        {
            for (; index < endIndex; index++)
            {
                switch (source[index])
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        return index;
                }
            }

            return endIndex - 1;
        }

        private int NextToKeyStart(int index)
        {
            int endIndex = startIndex + length;
            if (index >= endIndex)
                return -1;

            return NextToNoWhiteSpaces(index, endIndex);
        }

        private int NextToKeyEnd(int index)
        {
            int endIndex = startIndex + length;
            if (index >= endIndex)
                return -1;

            for (; index < endIndex; index++)
            {
                switch (source[index])
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                    case ':':
                        return index;
                }
            }

            return endIndex - 1;
        }

        private int NextToValueStart(int index)
        {
            int endIndex = startIndex + length;
            if (index >= endIndex)
                return -1;

            for (; index < endIndex; index++)
            {
                if (source[index] == ':')
                {
                    index++;
                    break;
                }
            }

            return NextToNoWhiteSpaces(index, endIndex);
        }
        #endregion

        #region Object
        public override int GetHashCode()
        {
            return source.GetHashCode() + startIndex + (10000 * length);
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
            if (source != null)
                return string.Format("JValue({0})", source.Substring(startIndex, length));
            else
                return "JValue(null)";
        }
        #endregion

        #region IComparable<JValue>
        public int CompareTo(JValue other)
        {
            return string.Compare(source, startIndex, other.source, other.startIndex, Math.Max(length, other.length));
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
            return value.AsSingle();
        }

        public static implicit operator double(JValue value)
        {
            return value.AsDouble();
        }

        public static implicit operator string(JValue value)
        {
            return value.AsString();
        }
        #endregion

        #region TestBed
        private static void Main(string[] args)
        {
            var source = new JValue(@"{
                name: ""Json guide"",
                pages: 400,
                authors: [""halak"", ""foo"", ""bar"", ""blah""]
            ");

            JValue book = new JValue(source);
            string name = book["name"];
            Console.WriteLine("Name: {0}", name);

            int pages = book["pages"];
            Console.WriteLine("Pages: {0}", pages);

            Console.WriteLine("Primary author: {0}", book["authors"][0].AsString());
            Console.WriteLine("Authors:");
            foreach (var item in book["authors"].Array())
                Console.WriteLine("\t{0}", item);
            Console.WriteLine("Unknown author: {0}", book["authors"][100].AsString());
        }
        #endregion
    }

    #region Utilities
    public static class JValueStringExtension
    {
        public static IEnumerable<string> SplitAndEnumerate(this string s, char c)
        {
            int start = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c)
                {
                    yield return s.Substring(start, i - start);
                    start = i + 1;
                }
            }

            yield return s.Substring(start);
        }
    }
    #endregion
}