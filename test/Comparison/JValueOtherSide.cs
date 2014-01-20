using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halak.JValueComparison
{
    static class JValueOtherSide
    {
        public static void EnumerateArray(string source)
        {
            var data = JValue.Parse(source).AsArray();
            foreach (var item in data)
            {
                Program.Noop(item.AsInt());
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = JValue.Parse(source).AsObject();
            foreach (var item in keys)
            {
                Program.Noop(data[item].AsInt());
            }
        }
    }
}
