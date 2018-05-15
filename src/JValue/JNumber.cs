using System;

namespace Halak
{
    public struct JNumber
    {
        private readonly JValue integerPart;
        private readonly JValue fractionalPart;
        private readonly JValue exponent;

        public JValue IntegerPart { get { return integerPart; } }
        public JValue FractionalPart { get { return fractionalPart; } }
        public JValue Exponent { get { return exponent; } }

        public JNumber(JValue integerPart, JValue fractionalPart, JValue exponent)
        {
            this.integerPart = integerPart;
            this.fractionalPart = fractionalPart;
            this.exponent = exponent;
        }
    }

    public struct JNumberParser
    {
        public static readonly JNumberParser NaN = new JNumberParser();
        public static readonly JNumberParser Zero = new JNumberParser(0, 1, 0, Flags.Valid);

        [Flags]
        private enum Flags : byte
        {
            Valid = 0x80,
            Negative = 0x40,
            Overflow = 0x20,
        }

        private readonly uint coefficient;
        private readonly Flags flags;
        private readonly byte digits;
        private readonly short exponent;

        public bool IsNaN { get { return flags == 0; } }
        public bool IsPositive { get { return !IsNegative; } }
        public bool IsNegative { get { return (flags & Flags.Negative) != 0; } }
        public bool IsOverflow { get { return (flags & Flags.Overflow) != 0; } }
        public int Digits { get { return digits; } }
        public int Exponent { get { return exponent; } }

        private JNumberParser(uint coefficient, int digits, int exponent, Flags flags)
        {
            if (digits > 10 || (digits == 10 && coefficient < 1000000000U))
                flags |= Flags.Overflow;

            this.coefficient = coefficient;
            this.digits = (byte)digits;
            this.exponent = (short)exponent;
            this.flags = flags;
        }

        public int ToInt32(int defaultValue = 0)
        {
            if (IsNaN || IsOverflow)
                return defaultValue;

            var i = coefficient;
            var e = Exponent;
            if (e != 0)
            {
                var il = Digits + e;
                if (il > 0)
                {
                    if (il <= UInt32Powers10.Length)
                    {
                        if (e > 0)
                        {
                            i *= Pow10ToUInt32(e);
                        }
                        else
                        {
                            i /= Pow10ToUInt32(-e);
                        }
                    }
                    else
                        return defaultValue;
                }
                else
                    return 0;
            }

            if (IsPositive)
                return i <= int.MaxValue ? (int)i : defaultValue;
            else
                return i <= 0x80000000 ? (int)-i : defaultValue;
        }

        public static JNumberParser Analyze(string s) { return Analyze(s, 0, s.Length); }
        public static JNumberParser Analyze(string s, int startIndex) { return Analyze(s, startIndex, s.Length - startIndex); }
        public static JNumberParser Analyze(string s, int startIndex, int length)
        {
            if (length <= 0)
                return NaN;

            var index = startIndex;
            var end = startIndex + length;
            var flags = Flags.Valid;
            var c = s[index++];
            if (c == '-')
            {
                flags |= Flags.Negative;
                c = s[index++];
            }
            if (IsDigit(c) == false)
                return NaN;

            var coefficient = 0U;
            var digits = 0;
            var exponent = 0;
            if (c == '0')
            {
                if (index == end || IsTerminal(s[index]))
                    return Zero;
                else if (s[index++] == '.')
                    goto FractionalPart;
                else
                    return NaN;
            }
            else
            {
                digits++;
                coefficient = ToUInt32(c);
            }

            if (index == end)
                return new JNumberParser(coefficient, digits, exponent, flags);

            const uint CoefficientBeforeOverflow = uint.MaxValue / 10 - 1;

            // --------------------------------------------------
            // IntegerPart
            do
            {
                c = s[index++];
                if (IsDigit(c))
                {
                    digits++;

                    if (coefficient <= CoefficientBeforeOverflow)
                        coefficient = (coefficient * 10) + ToUInt32(c);
                }
                else if (c == '.')
                    goto FractionalPart;
                else if (c == 'E' || c == 'e')
                    goto ExponentPart;
                else if (IsTerminal(c))
                    break;
                else
                    return NaN;
            } while (index < end);

            return new JNumberParser(coefficient, digits, 0, flags);

            // --------------------------------------------------
            FractionalPart:
            if (index == end)
                return NaN;
            if (digits == 0)
            {
                // Skip Leading-zero
                do
                {
                    c = s[index];
                    if (c == '0')
                    {
                        exponent--;
                        index++;
                    }
                    else if ('1' <= c && c <= '9')
                        break;
                    else if (c == 'E' || c == 'e')
                        goto ExponentPart;
                    else if (IsTerminal(c))
                        return Zero;
                    else
                        return NaN;
                } while (index < end);
            }

            do
            {
                c = s[index++];
                if (IsDigit(c))
                {
                    digits++;
                    exponent--;

                    if (coefficient <= CoefficientBeforeOverflow)
                        coefficient = (coefficient * 10) + ToUInt32(c);
                }
                else if (c == 'E' || c == 'e')
                    goto ExponentPart;
                else if (IsTerminal(c))
                    break;
                else
                    return NaN;
            } while (index < end);

            return new JNumberParser(coefficient, digits, exponent, flags);

            // --------------------------------------------------
            ExponentPart:
            if (index == end)
                return NaN;

            var negativeExponentSign = false;
            c = s[index++];
            if ((c == '-' || c == '+') && index < end)
            {
                negativeExponentSign = (c == '-');
                c = s[index++];
            }
            if (IsDigit(c) == false)
                return NaN;

            var explicitExponent = ToUInt32(c);
            while (index < end)
            {
                c = s[index++];
                if (IsDigit(c))
                    explicitExponent = (explicitExponent * 10) + ToUInt32(c);
                else if (IsTerminal(c))
                    break;
                else
                    return NaN;
            }

            if (digits > 0)
            {
                if (negativeExponentSign)
                    exponent -= (int)explicitExponent;
                else
                    exponent += (int)explicitExponent;

                return new JNumberParser(coefficient, digits, exponent, flags);
            }
            else
                return Zero;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool IsDigit(char c) { return '0' <= c && c <= '9'; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool IsTerminal(char c) { return c == ',' || c == '}' || c == ']' || c == ' ' || c == '\n' || c == '\r' || c == '\t' ; }
        public static uint ToUInt32(char c) { return (uint)(c - '0'); }

        public override int GetHashCode() { return (int)coefficient ^ (exponent << 16 | (int)flags << 8 | digits); }
        public override string ToString()
        {
            if (IsNaN)
                return double.NaN.ToString();

            if (IsOverflow == false)
            {
                if (exponent > 0)
                    return (coefficient * Math.Pow(10.0, exponent) * (IsNegative ? -1.0 : 1.0)).ToString();
                else
                    return new Decimal((int)coefficient, 0, 0, IsNegative, (byte)(-exponent)).ToString();
            }
            else
                return string.Concat("JNumber(", 0, ", ", 0, ")");
        }

        public static uint Pow10ToUInt32(int d) => UInt32Powers10[d];

        private static readonly uint[] UInt32Powers10 = new[]
        {
            1U, 10U, 100U, 1000U, 10000U, 100000U, 1000000U, 10000000U, 100000000U, 1000000000U,
        };
    }
}
