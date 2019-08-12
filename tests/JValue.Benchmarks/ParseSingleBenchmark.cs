using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class ParseSingleBenchmark
    {
        private string number1;
        private string number2;
        private NumberFormatInfo invariantInfo;

        [GlobalSetup]
        public void Setup()
        {
            number1 = "1234.56789";
            number2 = "1.1234e+10";
            invariantInfo = NumberFormatInfo.InvariantInfo;
        }

        [Benchmark(Description = "float.Parse", Baseline = true)]
        public float SystemSingleParse()
        {
            return float.Parse(number1) + float.Parse(number2);
        }

        [Benchmark(Description = "float.Parse(Invariant)")]
        public float SystemSingleParseInvariant()
        {
            return float.Parse(number1, invariantInfo) + float.Parse(number2, invariantInfo);
        }

        [Benchmark(Description = "JValue.Parse")]
        public float JValueParse()
        {
            return
                JNumber.ParseSingle(number1) +
                JNumber.ParseSingle(number2);
        }
    }
}
