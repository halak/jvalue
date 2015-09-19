using System;
using System.Collections.Generic;

namespace Halak.JValueComparison
{
    static class ServiceStackSide
    {
        public static void EnumerateArray(string source)
        {
            var data = ServiceStack.Text.JsonSerializer.DeserializeFromString<List<object>>(source);
            foreach (var item in data)
            {
                Program.Noop(item);
            }
        }

        public static void QueryObject(string source, IEnumerable<string> keys)
        {
            var data = ServiceStack.Text.JsonSerializer.DeserializeFromString<ServiceStack.Text.JsonObject>(source);
            foreach (var item in keys)
            {
                Program.Noop(data.Child(item));
            }
        }
    }
}
