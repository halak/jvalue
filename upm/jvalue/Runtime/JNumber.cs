using System;
using System.Globalization;

namespace Halak
{
    public partial struct JNumber : IEquatable<JNumber>, IComparable<JNumber>
    {
        public static readonly JNumber NaN = new JNumber(string.Empty, 0, 0, 0, 0);
        public static readonly JNumber Zero = new JNumber(0);
        public static readonly JNumber One = new JNumber(1);

        private readonly string source;
        private readonly int startIndex;
        private readonly int length;
        private readonly int toDecimalPoint;
        private readonly int toExponent;

        public bool IsNaN => source == null;
        public bool IsPositive => !IsNegative;
        public bool IsNegative => source != null && source[startIndex] == '-';
        public JValue IntegerPart => new JValue(source, startIndex, toDecimalPoint);
        public JValue FractionalPart => HasFractionalPart ? new JValue(source, FractionalPartIndex, FractionalPartLength) : JValue.Null;
        public JValue Exponent
        {
            get
            {
                if (HasExponent)
                {
                    var exponentIndex = startIndex + toExponent + 1;
                    if (exponentIndex == '+')
                        exponentIndex++;

                    return new JValue(source, exponentIndex, (startIndex + length) - exponentIndex);
                }
                else
                    return JValue.Null;
            }
        }

        public bool HasFractionalPart => toDecimalPoint < length;
        public bool HasExponent => toExponent < length;
        private int FractionalPartIndex => startIndex + toDecimalPoint + 1;
        private int FractionalPartLength => toExponent - toDecimalPoint - 1;

        public JNumber(int value) : this(value.ToString(NumberFormatInfo.InvariantInfo), true) { }
        public JNumber(long value) : this(value.ToString(NumberFormatInfo.InvariantInfo), true) { }
        public JNumber(float value) : this(value.ToString(NumberFormatInfo.InvariantInfo)) { }
        public JNumber(double value) : this(value.ToString(NumberFormatInfo.InvariantInfo)) { }
        public JNumber(decimal value) : this(value.ToString(NumberFormatInfo.InvariantInfo)) { }
        private JNumber(string source) : this(source, 0, source.Length, FindDecimalPoint(source), FindExponent(source)) { }
        private JNumber(string source, bool _ /* from integer */) : this(source, 0, source.Length, source.Length, source.Length) { }
        private JNumber(string source, int startIndex, int length, int toDecimalPoint, int toExponent)
        {
            this.source = source;
            this.startIndex = startIndex;
            this.length = length;
            this.toDecimalPoint = toDecimalPoint;
            this.toExponent = toExponent;
        }

        public int ToInt32(int defaultValue = default(int))
            => ParseInt32(source, startIndex, length, defaultValue);
        public int? ToNullableInt32()
            => ParseNullableInt32(source, startIndex, length);
        public long ToInt64(long defaultValue = default(long))
            => ParseInt64(source, startIndex, length, defaultValue);
        public long? ToNullableInt64()
            => ParseNullableInt64(source, startIndex, length);
        public float ToSingle(float defaultValue = default(float))
            => ParseSingle(source, startIndex, length, defaultValue);
        public float? ToNullableSingle()
            => ParseNullableSingle(source, startIndex, length);
        public double ToDouble(double defaultValue = default(double))
            => ParseDouble(source, startIndex, length, defaultValue);
        public double? ToNullableDouble()
            => ParseNullableDouble(source, startIndex, length);
        public decimal ToDecimal(decimal defaultValue = default(decimal))
            => ParseDecimal(source, startIndex, length, defaultValue);
        public decimal? ToNullableDecimal()
            => ParseNullableDecimal(source, startIndex, length);

        public bool Equals(JNumber other) => Equals(this, other);
        public int CompareTo(JNumber other) => Compare(this, other);
        public override bool Equals(object obj) => obj is JNumber other && Equals(this, other);
        public override int GetHashCode()
        {
            if (source == null)
                return 0;

            var hashCode = 0x457E453B;
            var end = startIndex + length;
            for (var i = startIndex; i < end; i++)
                hashCode = HashCode.Combine(hashCode, GetHashCode(source[i]));

            return hashCode;
        }

        public override string ToString() => source?.Substring(startIndex, length) ?? "NaN";

        public static bool TryParse(string s, int startIndex, out JNumber value)
        {
            value = NaN;

            if (startIndex >= s.Length)
                return false;

            var i = startIndex;
            var c = s[i++];
            var decimalPoint = -1;
            var exponentIndex = -1;
            if (c == '-' || IsDigit(c))
            { /* DO NOTHING */ }
            else if (c == '0' && (i < s.Length && s[i++] == '.'))
            {
                decimalPoint = i - 1;
                goto FractionalPart;
            }
            else
                return false;

            for (; i < s.Length; i++)
            {
                c = s[i];
                if (IsDigit(c))
                    continue;
                else if (c == '.')
                {
                    decimalPoint = i++;
                    goto FractionalPart;
                }
                else if (c == 'e' || c == 'E')
                {
                    exponentIndex = i++;
                    goto ExponentPart;
                }
                else if (IsTerminal(c))
                    goto Exit;
                else
                    return false;
            }

            goto Exit;

            FractionalPart:
            for (; i < s.Length; i++)
            {
                c = s[i];
                if (IsDigit(c))
                    continue;
                else if (c == 'e' || c == 'E')
                {
                    exponentIndex = i++;
                    goto ExponentPart;
                }
                else
                    return false;
            }

            goto Exit;

            ExponentPart:
            c = s[i++];
            if (c != '+' && c != '-' && IsDigit(c) == false)
                return false;

            for (; i < s.Length; i++)
            {
                if (IsDigit(s[i]) == false)
                    return false;
            }

            Exit:
            value = new JNumber(
                s,
                startIndex,
                i - startIndex,
                (decimalPoint != -1 ? decimalPoint : i) - startIndex,
                (exponentIndex != -1 ? exponentIndex : i) - startIndex);
            return true;
        }

        public static bool Equals(JNumber left, JNumber right)
        {
            return
                left.length == right.length &&
                string.CompareOrdinal(left.source, left.startIndex, right.source, right.startIndex, left.length) == 0;
        }

        public static int Compare(JNumber left, JNumber right)
        {
            var compareResult = left.length.CompareTo(right.length);
            if (compareResult != 0)
                return compareResult;
            else
                return string.CompareOrdinal(left.source, left.startIndex, right.source, right.startIndex, left.length);
        }

        private static int GetHashCode(char c)
        {
            switch (c)
            {
                case '0': return unchecked((int)0xD0DEB4A3);
                case '1': return unchecked((int)0xC949F3C9);
                case '2': return unchecked((int)0x88184C40);
                case '3': return unchecked((int)0xE920F181);
                case '4': return unchecked((int)0xBB0CC3A8);
                case '5': return unchecked((int)0x2098A99C);
                case '6': return unchecked((int)0x8291F7B0);
                case '7': return unchecked((int)0xFBF5112D);
                case '8': return unchecked((int)0xC3BB87D6);
                case '9': return unchecked((int)0x4AF33460);
                case '.': return unchecked((int)0x403FBC07);
                case '+': return unchecked((int)0xA2922785);
                case '-': return unchecked((int)0x9F91692A);
                case 'e': return unchecked((int)0xC0DFD050);
                case 'E': return unchecked((int)0xC0DFD050);
                default: return 0;
            }
        }

        private static int FindDecimalPoint(string s)
        {
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == '.')
                    return i;
            }

            return s.Length;
        }

        private static int FindExponent(string s)
        {
            for (var i = s.Length - 1; i >= 0; i--)
            {
                switch (s[i])
                {
                    case 'e':
                    case 'E':
                        return i;
                }
            }

            return s.Length;
        }

        public static bool operator ==(JNumber left, JNumber right) => Equals(left, right);
        public static bool operator !=(JNumber left, JNumber right) => Equals(left, right) == false;

        public static implicit operator JNumber(byte value) => new JNumber(value);
        public static implicit operator JNumber(short value) => new JNumber(value);
        public static implicit operator JNumber(int value) => new JNumber(value);
        public static implicit operator JNumber(long value) => new JNumber(value);
        public static implicit operator JNumber(float value) => new JNumber(value);
        public static implicit operator JNumber(double value) => new JNumber(value);
        public static implicit operator JNumber(decimal value) => new JNumber(value);
    }
}
