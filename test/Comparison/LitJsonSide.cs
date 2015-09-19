using System;
using System.Collections.Generic;

namespace Halak.JValueComparison
{
    static class LitJsonSide
    {
        public static void EnumerateArray(string source)
        {
            var data = LitJson.JsonMapper.ToObject(source);
            foreach (var item in data)
            {
                Program.Noop(item);
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = LitJson.JsonMapper.ToObject(source);
            foreach (var item in keys)
            {
                Program.Noop(data[item]);
            }
        }
    }
}
