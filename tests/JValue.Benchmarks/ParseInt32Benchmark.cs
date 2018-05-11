using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class ParseInt32Benchmark
    {
        private string shortNumber;
        private string longNumber;
        private string negativeNumber;
        private string minNumber;
        private string maxNumber;
        private NumberFormatInfo invariantInfo;

        [GlobalSetup]
        public void Setup()
        {
            shortNumber = "1";
            longNumber = "1487234734";
            negativeNumber = "-1123";
            minNumber = int.MinValue.ToString();
            maxNumber = int.MaxValue.ToString();
            invariantInfo = NumberFormatInfo.InvariantInfo;
        }

        [Benchmark(Description = "int.Parse", Baseline = true)]
        public int SystemInt32Parse()
        {
            return
                int.Parse(shortNumber) +
                int.Parse(longNumber) +
                int.Parse(negativeNumber) +
                int.Parse(minNumber) +
                int.Parse(maxNumber);
        }

        [Benchmark(Description = "int.TryParse")]
        public int SystemInt32TryParse()
        {
            int a, b, c, d, e;
            int.TryParse(shortNumber, out a);
            int.TryParse(longNumber, out b);
            int.TryParse(negativeNumber, out c);
            int.TryParse(minNumber, out d);
            int.TryParse(maxNumber, out e);
            return a + b + c + d + e;
        }

        [Benchmark(Description = "int.Parse(Invariant)")]
        public int SystemInt32ParseInvariant()
        {
            return
                int.Parse(shortNumber, invariantInfo) +
                int.Parse(longNumber, invariantInfo) +
                int.Parse(negativeNumber, invariantInfo) +
                int.Parse(minNumber, invariantInfo) +
                int.Parse(maxNumber, invariantInfo);
        }

        [Benchmark(Description = "JValue.Parse")]
        public int JValueParse()
        {
            return
                JsonHelper.Parse(shortNumber, 0, shortNumber.Length, 0) +
                JsonHelper.Parse(longNumber, 0, longNumber.Length, 0) +
                JsonHelper.Parse(negativeNumber, 0, negativeNumber.Length, 0) +
                JsonHelper.Parse(minNumber, 0, minNumber.Length, 0) +
                JsonHelper.Parse(maxNumber, 0, maxNumber.Length, 0);
        }
    }
}
