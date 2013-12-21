using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halak
{
    static class NewtonsoftJsonSide
    {
        static Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

        public static void EnumerateArray(string source)
        {
            var data = (object[])serializer.Deserialize(new System.IO.StringReader(source), typeof(object[]));
            foreach (var item in data)
            {
                Comparison.Noop(item);
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = (Dictionary<string, object>)serializer.Deserialize(new System.IO.StringReader(source), typeof(Dictionary<string, object>));
            foreach (var item in keys)
            {
                Comparison.Noop(data[item]);
            }
        }
    }
}
