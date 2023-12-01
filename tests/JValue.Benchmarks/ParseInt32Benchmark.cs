using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class ParseInt32Benchmark
    {
        private const NumberStyles jsonNumberStyles =
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowExponent;

        private string shortNumber;
        private string longNumber;
        private string negativeNumber;
        private string minNumber;
        private string maxNumber;
        private string exponentNumber;
        private NumberFormatInfo invariantInfo;

        [GlobalSetup]
        public void Setup()
        {
            shortNumber = "1";
            longNumber = "1487234734";
            negativeNumber = "-1123";
            minNumber = int.MinValue.ToString();
            maxNumber = int.MaxValue.ToString();
            exponentNumber = "1.23E2";
            invariantInfo = NumberFormatInfo.InvariantInfo;
        }

        [Benchmark(Description = "int.Parse", Baseline = true)]
        public int SystemInt32Parse()
        {
            return
                int.Parse(shortNumber, jsonNumberStyles) +
                int.Parse(longNumber, jsonNumberStyles) +
                int.Parse(negativeNumber, jsonNumberStyles) +
                int.Parse(minNumber, jsonNumberStyles) +
                int.Parse(maxNumber, jsonNumberStyles) +
                int.Parse(exponentNumber, jsonNumberStyles);
        }

        [Benchmark(Description = "int.TryParse")]
        public int SystemInt32TryParse()
        {
            if (int.TryParse(shortNumber, jsonNumberStyles, default, out var a) &&
                int.TryParse(longNumber, jsonNumberStyles, default, out var b) &&
                int.TryParse(negativeNumber, jsonNumberStyles, default, out var c) &&
                int.TryParse(minNumber, jsonNumberStyles, default, out var d) &&
                int.TryParse(maxNumber, jsonNumberStyles, default, out var e) &&
                int.TryParse(exponentNumber, jsonNumberStyles, default, out var f))
                return a + b + c + d + e + f;
            else
                return 0;
        }

        [Benchmark(Description = "int.Parse(Invariant)")]
        public int SystemInt32ParseInvariant()
        {
            return
                int.Parse(shortNumber, jsonNumberStyles, invariantInfo) +
                int.Parse(longNumber, jsonNumberStyles, invariantInfo) +
                int.Parse(negativeNumber, jsonNumberStyles, invariantInfo) +
                int.Parse(minNumber, jsonNumberStyles, invariantInfo) +
                int.Parse(maxNumber, jsonNumberStyles, invariantInfo) +
                int.Parse(exponentNumber, jsonNumberStyles, invariantInfo);
        }

        [Benchmark(Description = "JNumber.Parse")]
        public int JValueParse()
        {
            return
                JNumber.ParseInt32(shortNumber) +
                JNumber.ParseInt32(longNumber) +
                JNumber.ParseInt32(negativeNumber) +
                JNumber.ParseInt32(minNumber) +
                JNumber.ParseInt32(maxNumber) +
                JNumber.ParseInt32(exponentNumber);
        }
    }
}
