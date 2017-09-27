using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class ParseInt32Benchmark
    {
        private string shortNumber;
        private string longNumber;

        [GlobalSetup]
        public void Setup()
        {
            shortNumber = "1";
            longNumber = "1487234734";
        }

        [Benchmark(Description = "int.Parse", Baseline = true)]
        public int SystemInt32Parse()
        {
            return int.Parse(shortNumber) + int.Parse(longNumber);
        }

        [Benchmark(Description = "int.TryParse")]
        public int SystemInt32TryParse()
        {
            int a, b;
            int.TryParse(shortNumber, out a);
            int.TryParse(longNumber, out b);
            return a + b;
        }

        [Benchmark(Description = "JValue.Parse")]
        public int JValueParse()
        {
            return
                JsonHelper.Parse(shortNumber, 0, shortNumber.Length, 0) +
                JsonHelper.Parse(longNumber, 0, longNumber.Length, 0);
        }
    }
}
