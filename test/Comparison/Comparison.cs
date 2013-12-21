using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Halak
{
    static class Comparison
    {
        static Stopwatch sw = new Stopwatch();
        static void Main(string[] args)
        {
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.ProcessorAffinity = new IntPtr(2);
            currentProcess.PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            SmallIntArray();
            BigIntArray();
            SmallObject();
            BigObject();

            Console.WriteLine("Benchmark Complete");
            Console.ReadKey();
        }

        #region Execute
        static void Benchmark(string library, Action action, int count = 100000)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            action();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                action();
            }
            sw.Stop();

            Console.WriteLine("  {0}: {1:N0}ms", library, sw.ElapsedMilliseconds);
            action = null;
        }
        #endregion

        static void SmallIntArray()
        {
            string source = "[10, 20, 30, 40]";

            Console.WriteLine("Small Int Array");
            Benchmark("JValue", () => JValueSide.EnumerateArray(source));
            Benchmark("LitJson", () => LitJsonSide.EnumerateArray(source));
            Benchmark("JsonFx", () => JsonFxSide.EnumerateArray(source));
            Benchmark("Json.NET", () => NewtonsoftJsonSide.EnumerateArray(source));
            Console.WriteLine();
        }

        static void BigIntArray()
        {
            int count = 100;

            var random = new Random();
            var o = new int[10000];
            for (int i = 0; i < o.Length; i++)
                o[i] = random.Next();

            string source = '[' + string.Join(",", o) + ']';

            Console.WriteLine("Big Int Array");
            Benchmark("JValue", () => JValueSide.EnumerateArray(source), count);
            Benchmark("LitJson", () => LitJsonSide.EnumerateArray(source), count);
            Benchmark("JsonFx", () => JsonFxSide.EnumerateArray(source), count);
            Benchmark("Json.NET", () => NewtonsoftJsonSide.EnumerateArray(source), count);
            Console.WriteLine();
        }

        static void SmallObject()
        {
            string source = @"{
                ""Cupcake"": ""Android 1.5 (API Level 3)"",
                ""Donut"": ""Android 1.6 (API Level 4)"",
                ""Eclair"": ""Android 2.0 (API Level 5)"",
                ""Froyo"": ""Android 2.2 (API Level 8)"",
                ""Gingerbread"": ""Android 2.3(API Level 9)"",
                ""Honeycomb"": ""Android 3.0 (API Level 11)"",
                ""IceCreamSandwich"": ""Android 4.0 (API Level 14)"",
                ""JellyBean"": ""Android 4.1 (API Level 16)"",
                ""KitKat"": ""Android 4.4 (API Level 19)""
            }";
            var keys = new string[] { "JellyBean", "IceCreamSandwich", "Eclair", "Donut", "Cupcake", "Froyo", "Honeycomb", "KitKat" };

            int count = 10000;
            Console.WriteLine("Small Object");
            Benchmark("JValue", () => JValueSide.QueryObject(source, keys), count);
            Benchmark("LitJson", () => LitJsonSide.QueryObject(source, keys), count);
            Benchmark("JsonFx", () => JsonFxSide.QueryObject(source, keys), count);
            Benchmark("Json.NET", () => NewtonsoftJsonSide.QueryObject(source, keys), count);
            Console.WriteLine();
        }

        static void BigObject()
        {
            var random = new Random();
            var o = new HashSet<int>();
            while (o.Count < 100000)
                o.Add(random.Next());

            var allKeys = new List<string>();
            var sb = new StringBuilder(o.Count * 10);
            sb.Append("{");
            bool isFirst = true;
            foreach (var item in o)
            {
                if (isFirst == false)
                    sb.Append(",");
                else
                    isFirst = false;

                var key = string.Format("Hello{0}", item);
                allKeys.Add(key);
                sb.AppendFormat("\"{0}\": {1}", key, random.Next());
            }
            sb.Append("}");

            string[] keys = new string[8]
            {
                allKeys[allKeys.Count / 8 * 0],
                allKeys[allKeys.Count / 8 * 1],
                allKeys[allKeys.Count / 8 * 2],
                allKeys[allKeys.Count / 8 * 3],
                allKeys[allKeys.Count / 8 * 4],
                allKeys[allKeys.Count / 8 * 5],
                allKeys[allKeys.Count / 8 * 6],
                allKeys[allKeys.Count / 8 * 7],
            };

            string source = sb.ToString();
            int count = 10;

            Console.WriteLine("Big Object");
            Benchmark("JValue", () => JValueSide.QueryObject(source, keys), count);
            Benchmark("LitJson", () => LitJsonSide.QueryObject(source, keys), count);
            Benchmark("JsonFx", () => JsonFxSide.QueryObject(source, keys), count);
            Benchmark("Json.NET", () => NewtonsoftJsonSide.QueryObject(source, keys), count);
            Console.WriteLine();
        }

        public static void Noop()
        {
        }

        public static void Noop<T>(T value)
        {
        }
    }
}
