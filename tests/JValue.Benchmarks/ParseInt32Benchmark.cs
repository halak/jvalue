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
                JsonHelper.ParseInt32Old(shortNumber, 0, shortNumber.Length) +
                JsonHelper.ParseInt32Old(longNumber, 0, longNumber.Length) +
                JsonHelper.ParseInt32Old(negativeNumber, 0, negativeNumber.Length) +
                JsonHelper.ParseInt32Old(minNumber, 0, minNumber.Length) +
                JsonHelper.ParseInt32Old(maxNumber, 0, maxNumber.Length);
        }

        [Benchmark(Description = "JValue.ParseNew")]
        public int JValueParseNew()
        {
            return
                JsonHelper.ParseInt32New(shortNumber, 0, shortNumber.Length) +
                JsonHelper.ParseInt32New(longNumber, 0, longNumber.Length) +
                JsonHelper.ParseInt32New(negativeNumber, 0, negativeNumber.Length) +
                JsonHelper.ParseInt32New(minNumber, 0, minNumber.Length) +
                JsonHelper.ParseInt32New(maxNumber, 0, maxNumber.Length);
        }

        [Benchmark(Description = "JValue.ParseNewNew")]
        public int JValueParseNewNew()
        {
            return
                JsonHelper.ParseInt32NewNew(shortNumber, 0) +
                JsonHelper.ParseInt32NewNew(longNumber, 0) +
                JsonHelper.ParseInt32NewNew(negativeNumber, 0) +
                JsonHelper.ParseInt32NewNew(minNumber, 0) +
                JsonHelper.ParseInt32NewNew(maxNumber, 0);
        }
    }
}
