using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Halak
{
    class Development
    {
        static void Main(string[] args)
        {
            // PerformanceTest_IsInteger();

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

            sw.Start();
            for (int i = 0; i < count; i++)
            {
                test.IndexOf('.', startIndex, length);
                test.IndexOf('e', startIndex, length);
                test.IndexOf('E', startIndex, length);
            }
            sw.Stop();
            Console.WriteLine("IndexOf {0}ms", sw.ElapsedMilliseconds);

            char[] floatingPoint = { '.', 'e', 'E' };
            sw.Reset();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                test.IndexOfAny(floatingPoint, startIndex, length);
            }
            sw.Stop();
            Console.WriteLine("IndexOfAny {0}ms", sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                for (int k = startIndex; k < startIndex + length; k++)
                {
                    char c = test[k];
                    if (c == '.' || c == 'e' || c == 'E')
                        break;
                }
            }
            sw.Stop();
            Console.WriteLine("Handmade if {0}ms", sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < count; i++)
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
            }
            sw.Stop();
            Console.WriteLine("Handmade switch {0}ms", sw.ElapsedMilliseconds);

            Console.WriteLine();
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
