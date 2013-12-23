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
                if (string.IsNullOrEmpty(source) == false)
                    return GetTypeCode(source[startIndex]);
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
        public JValue(string source)
        {
            if (string.IsNullOrEmpty(source))
                source = string.Empty;

            this.source = source;
            this.startIndex = 0;
            this.length = source.Length;

            this.startIndex = SkipWhitespaces(0);
            this.length = source.Length - this.startIndex;
        }

        private JValue(string source, int startOffset, int length)
        {
            this.source = source;
            this.startIndex = startOffset;
            this.length = length;
        }
        #endregion

        #region Methods
        private static TypeCode GetTypeCode(char c)
        {
            switch (c)
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
                    return JValueExtension.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private bool IsInteger()
        {
            for (int i = startIndex; i < startIndex + length; i++)
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
                return JValueExtension.Parse(source, startIndex, length, defaultValue);
            else
                return (int)JValueExtension.Parse(source, startIndex, length, (double)defaultValue);
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
                    return JValueExtension.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private long AsLongActually(long defaultValue = 0)
        {
            if (IsInteger())
                return JValueExtension.Parse(source, startIndex, length, defaultValue);
            else
                return (long)JValueExtension.Parse(source, startIndex, length, (double)defaultValue);
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
                    return JValueExtension.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private float AsFloatActually(float defaultValue = 0.0f)
        {
            return JValueExtension.Parse(source, startIndex, length, defaultValue);
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
                    return JValueExtension.Parse(source, startIndex + 1, length - 2, defaultValue);
                default:
                    return defaultValue;
            }
        }

        private double AsDoubleActually(double defaultValue = 0.0)
        {
            return JValueExtension.Parse(source, startIndex, length, defaultValue);
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
            int count = 1;
            int depth = 0;
            int end = startIndex + length - 1; // ignore } or ]
            for (int i = startIndex + 1; i < end; i++) // ignore { or [
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
        /// 점(.)으로 이름을 구분하여 하위 객체의 하위 객체까지 탐색하여 가져올 수 있습니다.
        /// </summary>
        /// <param name="key">이름</param>
        /// <returns>입력한 이름에 존재하는 JValue 값. 탐색중에 하위 객체를 찾지 못하면 탐색을 중단하고 JValue.Null을 반환합니다.</returns>
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
            foreach (var item in key.SplitRangeAndEnumerate('.'))
                current = current.GetFromOneDepth(key, item.Start, item.Length);

            return current;
        }

        private JValue GetFromOneDepth(string key, int keyStartIndex, int keyLength)
        {
            if (Type == TypeCode.Object)
            {
                int end = startIndex + length - 1;

                int kStart = SkipWhitespaces(startIndex + 1);
                while (kStart < end)
                {
                    int kEnd = SkipString(kStart);
                    int vStart = SkipWhitespaces(kEnd + 1);
                    int vEnd = SkipValue(vStart);

                    kStart++; // remove quotes
                    kEnd--; // remove quotes

                    if (keyLength == kEnd - kStart &&
                        string.Compare(key, keyStartIndex, source, kStart, keyLength) == 0)
                    {
                        return new JValue(source, vStart, vEnd - vStart);
                    }

                    kStart = SkipWhitespaces(vEnd + 1);
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
                int end = startIndex + length - 1;

                int vStart = SkipWhitespaces(startIndex + 1);
                while (vStart < end)
                {
                    int vEnd = SkipValue(vStart);
                    yield return new JValue(source, vStart, vEnd - vStart);
                    vStart = SkipWhitespaces(vEnd + 1);
                }
            }
        }

        public IEnumerable<KeyValuePair<int, JValue>> IndexedArray()
        {
            if (Type == TypeCode.Array)
            {
                int end = startIndex + length - 1;

                int index = 0;
                int vStart = SkipWhitespaces(startIndex + 1);
                while (vStart < end)
                {
                    int vEnd = SkipValue(vStart);
                    yield return new KeyValuePair<int, JValue>(index++, new JValue(source, vStart, vEnd - vStart));
                    vStart = SkipWhitespaces(vEnd + 1);
                }
            }
        }

        public IEnumerable<KeyValuePair<string, JValue>> Object()
        {
            if (Type == TypeCode.Object)
            {
                int end = startIndex + length - 1;

                int kStart = SkipWhitespaces(startIndex + 1);
                while (kStart < end)
                {
                    int kEnd = SkipString(kStart);
                    int vStart = SkipWhitespaces(kEnd + 1);
                    int vEnd = SkipValue(vStart);

                    kStart++; // remove quotes
                    kEnd--; // remove quotes

                    yield return new KeyValuePair<string, JValue>(source.Substring(kStart, kEnd - kStart),
                                                                  new JValue(source, vStart, vEnd - vStart));
                    kStart = SkipWhitespaces(vEnd + 1);
                }
            }
        }

        private int SkipValue(int index)
        {
            int end = startIndex + length;
            if (end <= index)
                return end;

            var typeCode = GetTypeCode(source[index]);
            switch (typeCode)
            {
                case TypeCode.String:
                    return SkipString(index);
                case TypeCode.Array:
                case TypeCode.Object:
                    return SkipBracket(index);
                default:
                    return SkipLetterOrDigit(index);
            }
        }

        private int SkipWhitespaces(int index)
        {
            int end = startIndex + length;
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
            int end = startIndex + length;
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
            int end = startIndex + length;
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
            int end = startIndex + length;
            int depth = 0;
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
            if (source != null)
                return string.Format("JValue({0}, {1})", Type, source.Substring(startIndex, length));
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
        #endregion
    }

    #region Utilities
    public static class JValueExtension
    {
        public struct Range
        {
            public readonly int Start;
            public readonly int Length;

            public Range(int start, int length)
            {
                Start = start;
                Length = length;
            }
        }

        public static IEnumerable<Range> SplitRangeAndEnumerate(this string s, char c)
        {
            int start = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c)
                {
                    yield return new Range(start, i - start);
                    start = i + 1;
                }
            }

            yield return new Range(start, s.Length - start);
        }

        public static int Parse(string s, int startIndex, int length, int defaultValue)
        {
            int i = startIndex;
            if (s[startIndex] == '-')
                i++;

            int result = 0;
            length += startIndex;
            for (; i < length; i++)
            {
                if ('0' <= s[i] && s[i] <= '9')
                {
                    result = (result * 10) + (s[i] - 48);
                    if (result < 0) // is overflow
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
            int i = startIndex;
            if (s[startIndex] == '-')
                i++;

            long result = 0;
            length += startIndex;
            for (; i < length; i++)
            {
                if ('0' <= s[i] && s[i] <= '9')
                {
                    result = (result * 10) + (s[i] - 48);

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
            float value;
            if (float.TryParse(s.Substring(startIndex, length), out value))
                return value;
            else
                return defaultValue;
        }

        public static double Parse(string s, int startIndex, int length, double defaultValue)
        {
            double value;
            if (double.TryParse(s.Substring(startIndex, length), out value))
                return value;
            else
                return defaultValue;
        }
    }
    #endregion
}