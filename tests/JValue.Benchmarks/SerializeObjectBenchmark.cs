using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class SerializeObjectBenchmark
    {
        private object[] anonymousObjects;

        [Params(10, 1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            anonymousObjects = new object[]
            {
                new { X = 10, Y = 20, Z = 30 },
                new { A = 10.0, B = "Hello", C = "World", },
                new { Name = "Halak", Age = 30, Birthday = DateTime.Now, },
            };
        }

        [Benchmark(Description = "JValue", Baseline = true)]
        public StringBuilder TestJValue()
        {
            var halakJValue = new Halak.JsonConverter();
            var s = new StringBuilder(1024);
            for (var i = 0; i < N; i++)
            {
                foreach (var obj in anonymousObjects)
                {
                    s.AppendLine(halakJValue.FromObject(obj));
                    s.Clear();
                }
            }
            return s;
        }

        [Benchmark(Description = "Json.NET")]
        public StringBuilder TestJsonNET()
        {
            var newtonJsonNET = new Newtonsoft.Json.JsonSerializer() { ContractResolver =  new Newtonsoft.Json.Serialization.DefaultContractResolver() };
            var s = new StringBuilder(1024);
            for (var i = 0; i < N; i++)
            {
                foreach (var obj in anonymousObjects)
                {
                    using (var writer = new StringWriter(s))
                        newtonJsonNET.Serialize(writer, obj);
                    s.Clear();
                }
            }
            return s;
        }
    }
}
