using System;
using System.Text;

namespace Halak
{
    internal struct JsonNumberParser
    {
        [Flags]
        private enum ParserState : byte
        {
            Initial = 0x00,
            NegativeSign = 0x01,
            NegativeExponent = 0x02,
            IntegerPartFinished = 0x04,
            FractionalPartFinished = 0x08,
            ExponentFinished = 0x10,
            Finished = IntegerPartFinished | FractionalPartFinished | ExponentFinished,
            Error = 0x80,
            InvalidFormat = Error | 0x40 | Finished,
            TooBigExponent = Error | 0x20 | Finished,
        }

        private const int RegularExponent = 1000;

        private readonly string source;
        private int index;
        private ParserState state;
        private byte current;
        private byte digits;  // digits 따로 세지 말고 startIndex와 currentIndex의 차로 재보기
        private byte c;

        public int Index { get { return index; } }
        public bool IsPositive { get { return (state & ParserState.NegativeSign) == 0; } }
        public bool IsNegative { get { return (state & ParserState.NegativeSign) != 0; } }
        public bool HasIntegerPartDigit { get { return (state & ParserState.IntegerPartFinished) == 0; } }
        public bool HasFractionalPartDigit { get { return (state & ParserState.FractionalPartFinished) == 0; } }
        public bool HasExponent { get { return (state & ParserState.ExponentFinished) == 0; } }
        public bool IsFinished { get { return (state & ParserState.Finished) == ParserState.Finished; } }
        public bool HasError { get { return (state & ParserState.Error) != 0; } }
        public int Digits { get { return 0; } }

        public JsonNumberParser(string source, int index)
        {
            this.source = source;
            this.index = index;
            this.state = ParserState.Initial;
            this.current = 0;
            this.digits = 0;
            c = 0;
            Initialize();
        }

        private void Initialize()
        {
            if (index >= source.Length)
            {
                state = ParserState.InvalidFormat;
                return;
            }

            var c = source[index];
            if (c == '-')
            {
                if (++index < source.Length)
                    state = ParserState.NegativeSign;
                else
                {
                    state = ParserState.InvalidFormat;
                    return;
                }
            }

            c = source[index];
            if ('1' <= c && c <= '9')
            {
                current = ToDigit(c);
                return;
            }
            else if (c == '0')
            {
                if (index + 2 < source.Length && source[index + 1] == '.' && IsDigit(source[index + 2]))
                {
                    index += 2;
                    state |= ParserState.IntegerPartFinished;
                    current = ToDigit(source[index + 2]);
                }
                else
                    state |= ParserState.InvalidFormat;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public uint ReadDigitInIntegerPart()
        {
            var d = current;
            if (++index < source.Length)
            {
                var c = source[index];
                if (IsDigit(c))
                    current = ToDigit(c);
                else
                    ReadOtherPart(c);
            }
            else
                state |= ParserState.Finished;

            digits++;
            return d;
        }

        private void ReadOtherPart(char c)
        {
            if (c == '.')
            {
                if (MoveNextDigit())
                    state |= ParserState.IntegerPartFinished;
                else
                    state |= ParserState.InvalidFormat;
                return;
            }

            if (c == 'e' || c == 'E')
            {
                OnExponentCharacter();
                return;
            }

            if (IsTerminal(c))
            {
                state |= ParserState.Finished;
                return;
            }

            state |= ParserState.InvalidFormat;
        }

        public uint ReadDigitInFractionalPart()
        {
            var d = (uint)current;
            if (++index == source.Length)
            {
                state |= ParserState.Finished;
               digits++;
                return d;
            }

            var c = source[index];
            if (IsDigit(c))
            {
                current = ToDigit(c);
                digits++;
                return d;
            }

            if (c == 'e' || c == 'E')
            {
                OnExponentCharacter();
                digits++;
                return d;
            }

            if (IsTerminal(c))
            {
                state |= ParserState.Finished;
                digits++;
                return d;
            }

            state |= ParserState.InvalidFormat;
            return 0;
        }

        public int ReadExponent()
        {
            var e = (int)current;
            index++;
            while (index < source.Length)
            {
                var c = source[index++];
                if (IsDigit(c))
                {
                    e = (e * 10) + ToDigit(c);
                    if (e > 1000)
                    {
                        state |= ParserState.TooBigExponent;
                        return 0;
                    }
                }
                else if (IsTerminal(c))
                {
                    state |= ParserState.Finished;
                    break;
                }
                else
                {
                    state |= ParserState.InvalidFormat;
                    return 0;
                }
            }

            if (index == source.Length)
                state |= ParserState.Finished;

            return (state & ParserState.NegativeExponent) == 0 ? e : -e;
        }

        private void OnExponentCharacter()
        {
            if (++index < source.Length)
            {
                state |= ParserState.IntegerPartFinished | ParserState.FractionalPartFinished;

                var c = source[index];
                if (c == '-')
                {
                    state |= ParserState.NegativeExponent;
                    if (MoveNextDigit() == false)
                        state |= ParserState.InvalidFormat;
                }
                else if (c == '+')
                {
                    if (MoveNextDigit() == false)
                        state |= ParserState.InvalidFormat;
                }
                else if (IsDigit(c))
                {
                    current = ToDigit(c);
                }
                else
                    state |= ParserState.InvalidFormat;
            }
            else
                state |= ParserState.InvalidFormat;
        }

        private bool MoveNextDigit()
        {
            return ++index < source.Length && ((current = CheckDigit(source[index])) != byte.MaxValue);
        }

        private static bool IsDigit(char c) { return '0' <= c && c <= '9'; }
        private static byte ToDigit(char c) { return (byte)(c - '0'); }
        private static bool IsTerminal(char c) { return c == ',' || c == '}' || c == ']' || c == ' ' || c == '\n' || c == '\r' || c == '\t'; }
        private static byte CheckDigit(char c) { return '0' <= c && c <= '9' ? (byte)(c - '0') : byte.MaxValue; }
    }
}
