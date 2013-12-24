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
            // PerformanceTest_IsInteger();
            PerformanceTest_ParseInt();
            PerformanceTest_ParseFloat();
            PerformanceTest_ParseDouble();
            ParseNumberTest();

            /*
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
            */
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

        #region PerformanceTest (ParseNumber)
        static void PerformanceTest_ParseInt()
        {
            int count = 5000000;
            Benchmark("int.Parse", () => int.Parse("10000"), count);
            Benchmark("int.TryParse", () => { int x; int.TryParse("10000", out x); }, count);
            Benchmark("JValue.Parse", () => JValueExtension.Parse("10000", 0, 5, 0), count);
        }

        static void PerformanceTest_ParseDouble()
        {
            int count = 5000000;
            string source1 = "1234.56789";
            string source2 = "1.1234e+10";

            Benchmark("double.Parse", () =>
            {
                double.Parse(source1);
                double.Parse(source2);
            }, count);
            Benchmark("JValue.Parse", () =>
            {
                JValueExtension.Parse(source1, 0, source1.Length, 0.0);
                JValueExtension.Parse(source2, 0, source2.Length, 0.0);
            }, count);
        }

        static void PerformanceTest_ParseFloat()
        {
            int count = 5000000;
            string source1 = "1234.56789";
            string source2 = "1.1234e+10";

            Benchmark("double.Parse", () =>
            {
                float.Parse(source1);
                float.Parse(source2);
            }, count);
            Benchmark("JValue.Parse", () =>
            {
                JValueExtension.Parse(source1, 0, source1.Length, 0.0f);
                JValueExtension.Parse(source2, 0, source2.Length, 0.0f);
            }, count);
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

        static void ParseNumberTest()
        {
            ParseIntTest();
            ParseDoubleTest();
        }

        static void ParseIntTest()
        {
            Action<string, int> assert = (input, result) =>
            {
                Trace.Assert(JValueExtension.Parse(input, 0, input.Length, 0) == result);
            };
            assert("10000", 10000);
            assert("4294967295", 0); // overflow
            assert("2147483648", 0); // overflow
            assert("2147483647", 2147483647); // max
            assert("12387cs831", 0); // invalid
            assert("0", 0);
            assert("-12938723", -12938723);
            assert("3948222", 3948222);

            var random = new Random();
            for (int i = 0; i < 10000; i++)
            {
                var value = random.Next(int.MinValue, int.MaxValue);
                assert(value.ToString(), value);
            }
        }

        static void ParseDoubleTest()
        {
            Func<double, double, bool> almostEquals = (a, b) => Math.Abs(a - b) < 0.0000001;

            Action<string, double> assert = (input, result) =>
            {
                Trace.Assert(almostEquals(JValueExtension.Parse(input, 0, input.Length, 0.0), result));
            };
            assert("10000", 10000.0);
            assert("2147483647", 2147483647.0);
            assert("0", 0.0);
            assert("-1293.8723", -1293.8723);
            assert("3948.222", 3948.222);

            double min = -10000000.0;
            double max = +10000000.0;

            var specifiers = new string[] { "G", "E", "e" };
            var random = new Random();
            for (int i = 0; i < 100000; i++)
            {
                var value = Math.Round(random.NextDouble() * (max - min) + min, 6);
                assert(value.ToString(), value);

                var valueString = value.ToString(specifiers[random.Next(specifiers.Length)]);
                Trace.Assert(almostEquals(double.Parse(valueString), JValueExtension.Parse(valueString, 0, valueString.Length, 0.0)));
            }
        }
    }
}
