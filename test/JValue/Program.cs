using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.Data;
using System.Globalization;

namespace Halak.JValueDev
{
    public static class Program
    {
        static void Main(string[] args)
        {
            PerformanceTest_IsInteger();
            // PerformanceTest_ParseInt();

            Action<string, int> assertParseInt = (input, result) =>
            {
                Trace.Assert(JValueExtension.Parse(input, 0, input.Length, 0) == result);
            };
            assertParseInt("10000", 10000);
            assertParseInt("4294967295", 0); // overflow
            assertParseInt("2147483648", 0); // overflow
            assertParseInt("2147483647", 2147483647); // max
            assertParseInt("12387cs831", 0); // invalid
            assertParseInt("0", 0);
            assertParseInt("-12938723", -12938723);
            assertParseInt("3948222", 3948222);

            Trace.Assert(new JValue("true").Type == JValue.TypeCode.Boolean);
            Trace.Assert(new JValue("false").Type == JValue.TypeCode.Boolean);
            Trace.Assert(new JValue("10").Type == JValue.TypeCode.Number);
            Trace.Assert(new JValue("100").Type == JValue.TypeCode.Number);
            Trace.Assert(new JValue("10.0").Type == JValue.TypeCode.Number);
            Trace.Assert(new JValue("50.0").Type == JValue.TypeCode.Number);
            Trace.Assert(new JValue("\"Hello\"").Type == JValue.TypeCode.String);
            Trace.Assert(new JValue("\"World Hello\"").Type == JValue.TypeCode.String);

            Trace.Assert(new JValue("true").AsBoolean() == true);
            Trace.Assert(new JValue("false").AsBoolean() == false);
            Trace.Assert(new JValue("10").AsInt() == 10);

            BasicObjectTest1();
            BasicObjectTest2();
            BasicArrayTest1();
        }

        #region Benchmark
        public static void Benchmark(string name, Action action, int count = 100000)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            action();

            var oldMemory = GC.GetTotalMemory(false);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                action();
            }
            sw.Stop();

            var currentMemory = GC.GetTotalMemory(false);
            Console.WriteLine("  {0,-16}| {1,6:N0}ms | {2,10:N0}", name, sw.ElapsedMilliseconds, currentMemory - oldMemory);
            action = null;
        }
        #endregion

        #region PerformanceTest (IsInteger)
        static void PerformanceTest_IsInteger()
        {
            PerformanceTest_IsInteger("___ABCDFGHIJKLMNOPQRSTUVWXYZ___", 3, 25);
            PerformanceTest_IsInteger("___.BCDFGHIJKLMNOPQRSTUVWXYZ___", 3, 25);
            PerformanceTest_IsInteger("___eBCDFGHIJKLMNOPQRSTUVWXYZ___", 3, 25);
            PerformanceTest_IsInteger("___EBCDFGHIJKLMNOPQRSTUVWXYZ___", 3, 25);
            PerformanceTest_IsInteger("___ABCDFGHIJKLMNOPQRSTUVWXY.___", 3, 25);
            PerformanceTest_IsInteger("___ABCDFGHIJKLMNOPQRSTUVWXYe___", 3, 25);
            PerformanceTest_IsInteger("___ABCDFGHIJKLMNOPQRSTUVWXYE___", 3, 25);
        }

        static void PerformanceTest_IsInteger(string test, int startIndex, int length)
        {
            int count = 10000000;

            var sw = new Stopwatch();

            Console.WriteLine("Performance Test (IsInteger): {0}", test);

            Benchmark("IndexOf", () =>
                {
                    test.IndexOf('.', startIndex, length);
                    test.IndexOf('e', startIndex, length);
                    test.IndexOf('E', startIndex, length);
                }, count);

            char[] floatingPoint = { '.', 'e', 'E' };
            Benchmark("IndexOfAny", () =>
                {
                    test.IndexOfAny(floatingPoint, startIndex, length);
                }, count);

            Benchmark("Handmade if", () =>
                {
                    for (int k = startIndex; k < startIndex + length; k++)
                    {
                        char c = test[k];
                        if (c == '.' || c == 'e' || c == 'E')
                            break;
                    }
                }, count);

            Benchmark("Handmade switch", () =>
                {
                    for (int k = startIndex; k < startIndex + length; k++)
                    {
                        switch (test[k])
                        {
                            case '.':
                            case 'e':
                            case 'E':
                                k = startIndex + length;
                                break;
                        }
                    }
                }, count);
        }
        #endregion

        #region PerformanceTest (ParseInt)
        static void PerformanceTest_ParseInt()
        {
            int count = 3000000;
            Benchmark("int.Parse", () => int.Parse("10000"), count);
            Benchmark("int.TryParse", () => { int x; int.TryParse("10000", out x); }, count);
            Benchmark("JValue.Parse", () => JValueExtension.Parse("10000", 0, 5, 0), count);
        }
        #endregion

        static void BasicArrayTest1()
        {
            var a = new JValue(@"   [10,  20    ,  [10  ,30,40 ]     ,30 ,""Hello""  , ""ASDASD"", 1]");
            Trace.Assert(a.Type == JValue.TypeCode.Array);
            foreach (var item in a.Array())
                Console.WriteLine(item.ToString());
            foreach (var item in a.IndexedArray())
                Console.WriteLine("[{0}] = {1}", item.Key, item.Value.ToString());
        }

        static void BasicObjectTest1()
        {
            JValue people = new JValue(@"{
                ""first_name"": ""Mario"",
                ""last_name"":  ""Kim"",
                ""age"": 30,
                ""job"": ""Programmer""
            }");

            foreach (var item in people.Object())
            {
                Console.WriteLine("{0} = {1}", item.Key, item.Value.AsString());
            }
        }

        static void BasicObjectTest2()
        {
            JValue book = new JValue(@"{
                ""name"": ""Json guide"",
                ""pages"": 400,
                ""authors"": [""halak"", ""foo"", ""bar"", ""blah""]
            }");
            string name = book["name"];
            Console.WriteLine("Name: {0}", name);

            int pages = book["pages"];
            Console.WriteLine("Pages: {0}", pages);

            Console.WriteLine("Primary author: {0}", book["authors"][0].AsString());
            Console.WriteLine("Authors:");
            foreach (var item in book["authors"].Array())
                Console.WriteLine("\t{0}", item);
            Console.WriteLine("Unknown author: {0}", book["authors"][100].AsString());
        }
    }
}
