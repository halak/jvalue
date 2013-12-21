using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halak
{
    static class JsonFxSide
    {
        static JsonFx.Json.JsonReader reader = new JsonFx.Json.JsonReader();

        public static void EnumerateArray(string source)
        {
            dynamic data = reader.Read(source);
            foreach (var item in data)
            {
                Comparison.Noop(item);
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            dynamic data = reader.Read<Dictionary<string, object>>(source);
            foreach (var item in keys)
            {
                Comparison.Noop(data[item]);
            }
        }
    }
}
