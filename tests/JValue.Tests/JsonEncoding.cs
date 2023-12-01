using System.Globalization;
using System.Text;

namespace Halak
{
    public static class JsonEncoding
    {
        public static JValue EnsureAscii(string s)
            => JValue.Parse(string.Concat("\"", HexEscape(s), "\""));

        public static string HexEscape(string s)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c < 128)
                    stringBuilder.Append(c);
                else
                    stringBuilder.Append(HexEscape(c));
            }
            return stringBuilder.ToString();
        }

        private static string HexEscape(char c)
            => "\\u" + ((int)c).ToString("x4", NumberFormatInfo.InvariantInfo);
    }
}