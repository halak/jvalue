using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halak.JValueComparison
{
    static class MiniJSONSide
    {
        public static void EnumerateArray(string source)
        {
            var data = MiniJSON.Json.Deserialize(source) as List<object>;
            foreach (var item in data)
            {
                Program.Noop(item);
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = MiniJSON.Json.Deserialize(source) as Dictionary<string, object>;
            foreach (var item in keys)
            {
                Program.Noop(data[item]);
            }
        }
    }
}
