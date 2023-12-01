using BenchmarkDotNet.Running;

namespace Halak
{
    static class Program
    {
        static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(ParseInt32Benchmark),
                typeof(ParseSingleBenchmark),
                typeof(ParseDoubleBenchmark),
                typeof(SimpleObjectBenchmark),
            });
            switcher.Run(args);
        }
    }
}
