using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class LiteralParseTest
    {
        [TestMethod]
        public void ParseInt32()
        {
            int Parse(string s) => JNumber.ParseInt32(s);

            Assert.AreEqual(10000, Parse("10000"));
            Assert.AreEqual(0, Parse("4294967295"));  // overflow
            Assert.AreEqual(0, Parse("2147483648"));  // overflow
            Assert.AreEqual(2147483647, Parse("2147483647"));  // max
            Assert.AreEqual(0, Parse("12387cs831"));  // invalid
            Assert.AreEqual(0, Parse("0"));
            Assert.AreEqual(-12938723, Parse("-12938723"));
            Assert.AreEqual(3948222, Parse("3948222"));
            Assert.AreEqual(int.MinValue, Parse(int.MinValue.ToString()));
            Assert.AreEqual(int.MaxValue, Parse(int.MaxValue.ToString()));
            Assert.AreEqual(0, Parse((int.MinValue - 1L).ToString()));
            Assert.AreEqual(0, Parse((int.MaxValue + 1L).ToString()));

            var random = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var value = random.Next(int.MinValue, int.MaxValue);
                Assert.AreEqual(Parse(value.ToString()), value);
            }
        }

        [TestMethod]
        public void ParseInt64()
        {
            long Parse(string s) => JNumber.ParseInt64(s);

            Assert.AreEqual(10000L, Parse("10000"));
            Assert.AreEqual(0L, Parse("12387cs831"));  // invalid
            Assert.AreEqual(0L, Parse("0"));
            Assert.AreEqual(-12938723L, Parse("-12938723"));
            Assert.AreEqual(3948222L, Parse("3948222"));
            Assert.AreEqual(long.MinValue, Parse(long.MinValue.ToString()));
            Assert.AreEqual(long.MaxValue, Parse(long.MaxValue.ToString()));
            Assert.AreEqual(int.MinValue - 1L, Parse((int.MinValue - 1L).ToString()));
            Assert.AreEqual(int.MaxValue + 1L, Parse((int.MaxValue + 1L).ToString()));

            var random = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var value =
                    ((long)random.Next(int.MinValue, int.MaxValue) << 32) |
                    ((long)random.Next(int.MinValue, int.MaxValue));
                Assert.AreEqual(Parse(value.ToString()), value);
            }
        }

        [TestMethod]
        public void ParseSingle()
        {
            float Parse(string s) => JNumber.ParseSingle(s);

            Assert.AreEqual(10000.0f, Parse("10000"));
            Assert.AreEqual(0.0f, Parse("0"));
            Assert.AreEqual(-1293.8723f, Parse("-1293.8723"));
            Assert.AreEqual(3948.222f, Parse("3948.222"));
            Assert.AreEqual(123.45e+6f, Parse("123.45e+6"));
            Assert.AreEqual(123.45e6f, Parse("123.45e6"));
            Assert.AreEqual(1E3f, Parse("1E3"));

            const float Epsilon = 0.00001f;
            Assert.AreEqual(7.96153846f, Parse("7.96153846"), Epsilon);
            Assert.AreEqual(7.961538461f, Parse("7.961538461"), Epsilon);
            Assert.AreEqual(7.9615384615f, Parse("7.9615384615"), Epsilon);
            Assert.AreEqual(7.96153846153f, Parse("7.96153846153"), Epsilon);
            Assert.AreEqual(7.961538461538f, Parse("7.961538461538"), Epsilon);
            Assert.AreEqual(7.9615384615384635f, Parse("7.9615384615384635"), Epsilon);
            Assert.AreEqual(7.9615384615384635f, JNumber.ParseSingle("{\"hello\": 7.9615384615384635}", 10), Epsilon);
        }

        [DataTestMethod]
        [DataRow(10000.0, "10000")]
        [DataRow(2147483647.0, "2147483647")]
        [DataRow(0.0, "0")]
        [DataRow(-1293.8723, "-1293.8723")]
        [DataRow(3948.222, "3948.222")]
        [DataRow(123.45e+6, "123.45e+6")]
        [DataRow(123.45e6, "123.45e6")]
        [DataRow(1E3, "1E3")]
        public void ParseDouble(double expected, string input)
            => Assert.AreEqual(expected, JNumber.ParseDouble(input));

        [TestMethod]
        public void ParseRandomDouble()
        {
            double Parse(string s) => JNumber.ParseDouble(s);

            var min = -10000000.0;
            var max = +10000000.0;

            var specifiers = new string[] { "G", "E", "e" };
            var random = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var value = Math.Round(random.NextDouble() * (max - min) + min, 6);

                Assert.AreEqual(value, Parse(value.ToString()));
                var valueString = value.ToString(specifiers[random.Next(specifiers.Length)]);
                Assert.AreEqual(double.Parse(valueString), Parse(valueString), 0.000001);
            }
        }
    }
}
