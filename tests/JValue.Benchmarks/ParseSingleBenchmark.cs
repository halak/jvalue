using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class ParseSingleBenchmark
    {
        private string source1;
        private string source2;

        [GlobalSetup]
        public void Setup()
        {
            source1 = "1234.56789";
            source2 = "1.1234e+10";
        }

        [Benchmark(Description = "float.Parse", Baseline = true)]
        public float SystemSingleParse()
        {
            return float.Parse(source1) + float.Parse(source2);
        }

        [Benchmark(Description = "JValue.Parse")]
        public float JValueParse()
        {
            return
                JValueExtensions.Parse(source1, 0, source1.Length, 0.0f) +
                JValueExtensions.Parse(source2, 0, source2.Length, 0.0f);
        }
    }
}
