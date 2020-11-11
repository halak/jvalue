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
            if (int.TryParse(shortNumber, out var a) &&
                int.TryParse(longNumber, out var b) &&
                int.TryParse(negativeNumber, out var c) &&
                int.TryParse(minNumber, out var d) &&
                int.TryParse(maxNumber, out var e))
                return a + b + c + d + e;
            else
                return 0;
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
                JNumber.ParseInt32(shortNumber) +
                JNumber.ParseInt32(longNumber) +
                JNumber.ParseInt32(negativeNumber) +
                JNumber.ParseInt32(minNumber) +
                JNumber.ParseInt32(maxNumber);
        }
    }
}
