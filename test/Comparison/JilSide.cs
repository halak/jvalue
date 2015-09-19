using System;
using System.Collections.Generic;

namespace Halak.JValueComparison
{
    static class JilSide
    {
        public static void EnumerateArray(string source)
        {
            var data = Jil.JSON.Deserialize<List<object>>(source);
            foreach (var item in data)
            {
                Program.Noop(item);
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = Jil.JSON.Deserialize<Dictionary<string, object>>(source);
            foreach (var item in keys)
            {
                Program.Noop(data[item]);
            }
        }
    }
}
