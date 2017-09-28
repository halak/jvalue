using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class LiteralParseTest
    {
        [TestMethod]
        public void TestIntParsing()
        {
            int Parse(string s) => JsonHelper.Parse(s, 0, s.Length, 0);

            Assert.AreEqual(Parse("10000"), 10000);
            Assert.AreEqual(Parse("4294967295"), 0);  // overflow
            Assert.AreEqual(Parse("2147483648"), 0);  // overflow
            Assert.AreEqual(Parse("2147483647"), 2147483647);  // max
            Assert.AreEqual(Parse("12387cs831"), 0);  // invalid
            Assert.AreEqual(Parse("0"), 0);
            Assert.AreEqual(Parse("-12938723"), -12938723);
            Assert.AreEqual(Parse("3948222"), 3948222);

            var random = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var value = random.Next(int.MinValue, int.MaxValue);
                Assert.AreEqual(Parse(value.ToString()), value);
            }
        }

        [TestMethod]
        public void TestDoubleParsing()
        {
            double Parse(string s) => JsonHelper.Parse(s, 0, s.Length, 0.0);

            Assert.AreEqual(Parse("10000"), 10000.0);
            Assert.AreEqual(Parse("2147483647"), 2147483647.0);
            Assert.AreEqual(Parse("0"), 0.0);
            Assert.AreEqual(Parse("-1293.8723"), -1293.8723);
            Assert.AreEqual(Parse("3948.222"), 3948.222);

            var min = -10000000.0;
            var max = +10000000.0;

            var specifiers = new string[] { "G", "E", "e" };
            var random = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var value = Math.Round(random.NextDouble() * (max - min) + min, 6);

                Assert.AreEqual(Parse(value.ToString()), value);
                var valueString = value.ToString(specifiers[random.Next(specifiers.Length)]);
                Assert.AreEqual(
                    double.Parse(valueString),
                    JsonHelper.Parse(valueString, 0, valueString.Length, 0.0));
            }
        }
    }
}
