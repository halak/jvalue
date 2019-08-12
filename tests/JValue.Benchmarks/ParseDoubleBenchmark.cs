using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class ParseDoubleBenchmark
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

        [Benchmark(Description = "double.Parse", Baseline = true)]
        public double SystemDoubleParse()
        {
            return double.Parse(number1) + double.Parse(number2);
        }

        [Benchmark(Description = "double.Parse(Invariant)")]
        public double SystemDoubleParseInvariant()
        {
            return double.Parse(number1, invariantInfo) + double.Parse(number2, invariantInfo);
        }

        [Benchmark(Description = "JValue.Parse")]
        public double JValueParse()
        {
            return
                JNumber.ParseDouble(number1) +
                JNumber.ParseDouble(number2);
        }
    }
}
