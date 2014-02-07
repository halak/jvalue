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
            //PerformanceTest_IsInteger();
            //PerformanceTest_ParseInt();
            //PerformanceTest_ParseFloat();
            //PerformanceTest_ParseDouble();
            //ParseNumberTest();

            Console.WriteLine(JValue.Parse(@"""\ub9c8\ub9b0""").AsString());

            Trace.Assert(JValue.Parse("true").Type == JValue.TypeCode.Boolean);
            Trace.Assert(JValue.Parse("false").Type == JValue.TypeCode.Boolean);
            Trace.Assert(JValue.Parse("10").Type == JValue.TypeCode.Number);
            Trace.Assert(JValue.Parse("100").Type == JValue.TypeCode.Number);
            Trace.Assert(JValue.Parse("10.0").Type == JValue.TypeCode.Number);
            Trace.Assert(JValue.Parse("50.0").Type == JValue.TypeCode.Number);
            Trace.Assert(JValue.Parse("\"Hello\"").Type == JValue.TypeCode.String);
            Trace.Assert(JValue.Parse("\"World Hello\"").Type == JValue.TypeCode.String);
            Trace.Assert(JValue.Parse("").Type == JValue.TypeCode.Null);
            Trace.Assert(JValue.Parse("null").Type == JValue.TypeCode.Null);
            Trace.Assert(new JValue().Type == JValue.TypeCode.Null);
            
            Trace.Assert(JValue.Parse("true").AsBoolean() == true);
            Trace.Assert(JValue.Parse("false").AsBoolean() == false);
            Trace.Assert(JValue.Parse("10").AsInt() == 10);

            MechanismTest();
            SimpleTest();
            BasicObjectTest1();
            BasicObjectTest2();
            BasicArrayTest1();
            

            Trace.Assert(new JValue(10).Type == JValue.TypeCode.Number);
            Trace.Assert(new JValue(10).AsInt() == 10);
            Trace.Assert(new JValue("Hello\nWorld").AsString() == "Hello\nWorld");
            Trace.Assert(new JValue(new List<JValue>() { 10, 20, 30 }).Type == JValue.TypeCode.Array);
            Trace.Assert(new JValue(new List<JValue>() { 10, 20, 30 }).ToString() == "[10,20,30]");

            DJValueTest();
        }

        #region Benchmark
        public static long Benchmark(string name, Action action, int count = 100000)
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
            Console.WriteLine("  {0,-16}| {1,10:N0}ms | {2,10:N0}", name, sw.ElapsedMilliseconds, currentMemory - oldMemory);
            action = null;

            return sw.ElapsedMilliseconds;
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

        static void MechanismTest()
        {
            JValue person = JValue.Parse(@"{""name"":""John"", ""age"":27, ""friend"":{""name"":""Tom""}}");
            JValue name = person["name"];
            JValue age = person["age"];
            JValue friend = person["friend"];
            JValue friendName = friend["name"];

            string nameValue = (string)name;
            int ageValue = (int)age;
        }

        static void SimpleTest()
        {
            JValue location = JValue.Parse(@"{""x"":10, ""y"":20}");
            var x = location["x"];
            Console.WriteLine((int)location["x"]); // 10
            Console.WriteLine((int)location["y"]); // 20
            Console.WriteLine(location["z"].ToString()); // null

            var person = new Dictionary<string, JValue>()
            {
                {"name", "John"},
                {"age", 27},
            };
            Console.WriteLine(new JValue(person).ToString()); // {"name":"John","age":27}
        }

        static void BasicArrayTest1()
        {
            var a = JValue.Parse(@"   [10,  20    ,  [10  ,30,40 ]     ,30 ,""Hello""  , ""ASDASD"", 1]");
            Trace.Assert(a.Type == JValue.TypeCode.Array);
            foreach (var item in a.Array())
                Console.WriteLine(item.ToString());
            foreach (var item in a.IndexedArray())
                Console.WriteLine("[{0}] = {1}", item.Key, item.Value.ToString());
        }

        static void BasicObjectTest1()
        {
            JValue people = JValue.Parse(@"{
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
            JValue book = JValue.Parse(@"{
                ""name"": ""Json guide"",
                ""pages"": 400,
                ""tags"": [""computer"", ""data-interchange format"", ""easy""],
                ""price"": {""usd"":0.99, ""krw"":1000}
            }");
            string name = book["name"];
            Console.WriteLine("Name: {0}", name);

            int pages = book["pages"];
            Console.WriteLine("Pages: {0}", pages);

            Console.WriteLine("Primary tag: {0}", book["tags"][0].AsString());
            Console.WriteLine("Tags:");
            foreach (var item in book["tags"].Array())
                Console.WriteLine("\t{0}", item);
            Console.WriteLine("Unknown author: {0}", book["tags"][100].AsString());

            JValue price = book["price"];
            Console.WriteLine("Price: ${0}", (double)price["usd"]);
            Console.WriteLine("");
            Console.WriteLine("Reserialization");
            Console.WriteLine(book.Serialize(false));
        }

        #region ParseNumberTest
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
        #endregion

        #region DJValue
        static void DJValueTest()
        {
            dynamic book = new DJValue(@"{
                ""name"": ""Json guide"",
                ""pages"": 400,
                ""authors"": [""halak"", ""foo"", ""bar"", ""blah""]
            }");

            Console.WriteLine("Name: {0}", book.name);
            Console.WriteLine("Pages: {0}", (int)book.pages);

            Console.WriteLine("Primary author: {0}", book.authors[0].AsString());
            Console.WriteLine("Authors:");
            foreach (var item in book.authors)
                Console.WriteLine("\t{0}", item);
            Console.WriteLine("Unknown author: {0}", book.authors[100].AsString());
        }
        #endregion
    }
}
