using System;
using System.Globalization;

namespace Halak
{
    public struct JNumber : IEquatable<JNumber>
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

        public JNumber(int value) : this(value.ToString(CultureInfo.InvariantCulture), true) { }
        public JNumber(long value) : this(value.ToString(CultureInfo.InvariantCulture), true) { }
        public JNumber(float value) : this(value.ToString(CultureInfo.InvariantCulture)) { }
        public JNumber(double value) : this(value.ToString(CultureInfo.InvariantCulture)) { }
        public JNumber(decimal value) : this(value.ToString(CultureInfo.InvariantCulture)) { }
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
            => JsonHelper.ParseInt32(source, startIndex, length, defaultValue);
        public int? ToNullableInt32()
            => JsonHelper.ParseNullableInt32(source, startIndex, length);
        public long ToInt64(long defaultValue = default(long))
            => JsonHelper.ParseInt64(source, startIndex, length, defaultValue);
        public long? ToNullableInt64()
            => JsonHelper.ParseNullableInt64(source, startIndex, length);
        public float ToSingle(float defaultValue = default(float))
            => JsonHelper.ParseSingle(source, startIndex, length, defaultValue);
        public float? ToNullableSingle()
            => JsonHelper.ParseNullableSingle(source, startIndex, length);
        public double ToDouble(double defaultValue = default(double))
            => JsonHelper.ParseDouble(source, startIndex, length, defaultValue);
        public double? ToNullableDouble()
            => JsonHelper.ParseNullableDouble(source, startIndex, length);
        public decimal ToDecimal(decimal defaultValue = default(decimal))
            => JsonHelper.ParseDecimal(source, startIndex, length, defaultValue);
        public decimal? ToNullableDecimal()
            => JsonHelper.ParseNullableDecimal(source, startIndex, length);

        public override bool Equals(object obj) => obj is JNumber other && Equals(this, other);
        public bool Equals(JNumber other) => Equals(this, other);
        public override int GetHashCode()
        {
            if (source == null)
                return -1;

            var hashCode = GetHashCode(source[startIndex]);
            var end = startIndex + length;
            for (var i = startIndex + 1; i < end; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ GetHashCode(source[i]);

            return (int)hashCode;
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
            if (c == '-' || JsonHelper.IsDigit(c))
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
                if (JsonHelper.IsDigit(c))
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
                else if (JsonHelper.IsTerminal(c))
                    goto Exit;
                else
                    return false;
            }

            goto Exit;

            FractionalPart:
            for (; i < s.Length; i++)
            {
                c = s[i];
                if (JsonHelper.IsDigit(c))
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
            if (c != '+' && c != '-' && JsonHelper.IsDigit(c) == false)
                return false;

            for (; i < s.Length; i++)
            {
                if (JsonHelper.IsDigit(s[i]) == false)
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

        private static uint GetHashCode(char c)
        {
            switch (c)
            {
                case '0': return 0xD0DEB4A3;
                case '1': return 0xC949F3C9;
                case '2': return 0x88184C40;
                case '3': return 0xE920F181;
                case '4': return 0xBB0CC3A8;
                case '5': return 0x2098A99C;
                case '6': return 0x8291F7B0;
                case '7': return 0xFBF5112D;
                case '8': return 0xC3BB87D6;
                case '9': return 0x4AF33460;
                case '.': return 0x403FBC07;
                case '+': return 0xA2922785;
                case '-': return 0x9F91692A;
                case 'e': return 0xC0DFD050;
                case 'E': return 0x2F2B6D75;
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
