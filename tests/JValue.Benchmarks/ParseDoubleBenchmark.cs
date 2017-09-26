using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class ParseDoubleBenchmark
    {
        private string source1;
        private string source2;

        [GlobalSetup]
        public void Setup()
        {
            source1 = "1234.56789";
            source2 = "1.1234e+10";
        }

        [Benchmark(Description = "double.Parse", Baseline = true)]
        public double SystemDoubleParse()
        {
            return double.Parse(source1) + double.Parse(source2);
        }

        [Benchmark(Description = "JValue.Parse")]
        public double JValueParse()
        {
            return
                JValueExtensions.Parse(source1, 0, source1.Length, 0.0) +
                JValueExtensions.Parse(source2, 0, source2.Length, 0.0);
        }
    }
}
