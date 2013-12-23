using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halak.JValueComparison
{
    static class JValueSide
    {
        public static void EnumerateArray(string source)
        {
            var data = new JValue(source);
            foreach (var item in data.Array())
            {
                Program.Noop(item.AsInt());
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = new JValue(source);
            foreach (var item in keys)
            {
                Program.Noop(data[item].AsInt());
            }
        }
    }
}
