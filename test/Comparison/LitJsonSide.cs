using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halak
{
    static class LitJsonSide
    {
        public static void EnumerateArray(string source)
        {
            var data = LitJson.JsonMapper.ToObject(source);
            foreach (var item in data)
            {
                Comparison.Noop(item);
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = LitJson.JsonMapper.ToObject(source);
            foreach (var item in keys)
            {
                Comparison.Noop(data[item]);
            }
        }
    }
}
