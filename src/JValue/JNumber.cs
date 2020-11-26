using System;
using System.Diagnostics;
using System.Globalization;

namespace Halak
{
    [DebuggerDisplay("{ToString(),nq}")]
    public partial struct JNumber : IComparable<JNumber>, IEquatable<JNumber>
    {
        public static readonly JNumber NaN = new JNumber(string.Empty, 0, 0, 0, 0);
        public static readonly JNumber Zero = new JNumber(0);
        public static readonly JNumber One = new JNumber(1);

        private readonly string source;
        private readonly int startIndex;
        private readonly int length;
        private readonly int decimalPointOffset;
        private readonly int exponentOffset;

        public JNumber(int value) : this(value.ToString(NumberFormatInfo.InvariantInfo), true) { }
        public JNumber(long value) : this(value.ToString(NumberFormatInfo.InvariantInfo), true) { }
        public JNumber(float value) : this(value.ToString(NumberFormatInfo.InvariantInfo)) { }
        public JNumber(double value) : this(value.ToString(NumberFormatInfo.InvariantInfo)) { }
        public JNumber(decimal value) : this(value.ToString(NumberFormatInfo.InvariantInfo)) { }
        private JNumber(string source) : this(source, 0, source.Length, FindDecimalPoint(source), FindExponent(source)) { }
        private JNumber(string source, bool _ /* from integer */) : this(source, 0, source.Length) { }
        private JNumber(string source, int startIndex, int length) : this(source, startIndex, length, length, length) { }
        private JNumber(string source, int startIndex, int length, int decimalPointOffset, int exponentOffset)
        {
            this.source = source;
            this.startIndex = startIndex;
            this.length = length;
            this.decimalPointOffset = decimalPointOffset;
            this.exponentOffset = exponentOffset;
        }

        public bool IsNaN => length == 0;
        public bool IsPositive => length > 0 && source[startIndex] != '-';
        public bool IsNegative => length > 0 && source[startIndex] == '-';
        public JNumber IntegerPart => new JNumber(source, startIndex, Math.Min(decimalPointOffset, exponentOffset));
        public JNumber FractionalPart => HasFractionalPart ? new JNumber(source, FractionalPartIndex, FractionalPartLength) : NaN;
        public JNumber Exponent
        {
            get
            {
                if (HasExponent)
                {
                    var exponentIndex = startIndex + exponentOffset + 1;

                    if (source[exponentIndex] == '+')
                        exponentIndex++;

                    return new JNumber(source, exponentIndex, (startIndex + length) - exponentIndex);
                }
                else
                    return NaN;
            }
        }

        public bool HasFractionalPart => decimalPointOffset < length;
        public bool HasExponent => exponentOffset < length;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int LeadingZeros
        {
            get
            {
                var index = startIndex;
                var endIndex = startIndex + length;
                while (index < endIndex && source[index] == '0')
                    index++;

                return index - startIndex;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int FractionalPartIndex => startIndex + decimalPointOffset + 1;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int FractionalPartLength
        {
            get
            {
                // Trim trailing zeros
                var index = startIndex + exponentOffset - 1;
                var endIndex = startIndex + decimalPointOffset;
                while (index > endIndex && source[index] == '0')
                    index--;

                return index - endIndex;
            }
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

        public int CompareTo(JNumber other) => Compare(this, other);
        public bool Equals(JNumber other) => Equals(this, other);
        public override bool Equals(object obj) => obj is JNumber other && Equals(this, other);
        public override int GetHashCode()
        {
            if (source == null)
                return 0;

            var hashCode = 0x457E453B;
            var endIndex = startIndex + length;
            for (var i = startIndex; i < endIndex; i++)
                hashCode = HashCode.Combine(hashCode, GetHashCode(source[i]));

            return hashCode;
        }

        public override string ToString() => source != null && length > 0 ? source.Substring(startIndex, length) : "NaN";

        public static JNumber Parse(string s) => Parse(s, 0);
        public static JNumber Parse(string s, int startIndex)
        {
            if (TryParse(s, startIndex, out var value))
                return value;
            else
                return NaN;
        }

        public static bool TryParse(string s, int startIndex, out JNumber value)
        {
            value = NaN;

            if (s == null)
                return false;
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

        public static int Compare(JNumber left, JNumber right)
        {
            var ordering = left.length.CompareTo(right.length);
            return ordering != 0 ? ordering : string.CompareOrdinal(left.source, left.startIndex, right.source, right.startIndex, left.length);
        }

        public static bool Equals(JNumber left, JNumber right)
        {
            return
                EqualsCore(left.IntegerPart, right.IntegerPart) &&
                EqualsCore(left.FractionalPart, right.FractionalPart) &&
                EqualsCore(left.Exponent, right.Exponent);
        }

        private static bool EqualsCore(JNumber left, JNumber right)
        {
            return
                left.length == right.length &&
                string.CompareOrdinal(left.source, left.startIndex, right.source, right.startIndex, left.length) == 0;
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
            var index = s.IndexOf('.');
            return index != -1 ? index : s.Length;
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
