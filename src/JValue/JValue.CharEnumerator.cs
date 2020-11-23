using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Halak
{
    partial struct JValue
    {
        public struct CharEnumerator : IEnumerator<char>
        {
            private readonly string source;
            private readonly int startIndex;
            private char current;
            private int index;

            internal CharEnumerator(JValue value) : this(value.source, value.startIndex) { }
            internal CharEnumerator(string source, int startIndex)
            {
                this.source = source;
                this.startIndex = startIndex;
                this.current = '\0';
                this.index = startIndex;
            }

            public char Current => current;
            object IEnumerator.Current => Current;

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
                        if (++index < source.Length)
                        {
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
                                    if (index + 4 < source.Length)
                                    {
                                        var a = source[++index];
                                        var b = source[++index];
                                        var c = source[++index];
                                        var d = source[++index];
                                        current = (char)((Hex(a) << 12) | (Hex(b) << 8) | (Hex(c) << 4) | (Hex(d)));
                                    }
                                    else
                                    {
                                        // Invalid Json
                                        current = '\uFFFD';
                                    }
                                    break;
                            }

                            return true;
                        }
                        else
                            return false;
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

            void IDisposable.Dispose() { }

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
