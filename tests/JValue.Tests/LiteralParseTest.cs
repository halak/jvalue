using System;
using System.Globalization;
using NUnit.Framework;

namespace Halak
{
    public class LiteralParseTest
    {
        [Test]
        public void ParseInt32()
        {
            static int Parse(string s) => JNumber.ParseInt32(s);

            Assert.That(Parse("10000"), Is.EqualTo(10000));
            Assert.That(Parse("4294967295"), Is.EqualTo(0)); // overflow
            Assert.That(Parse("2147483648"), Is.EqualTo(0)); // overflow
            Assert.That(Parse("-2147483649"), Is.EqualTo(0)); // overflow
            Assert.That(Parse("2147483647"), Is.EqualTo(2147483647)); // max
            Assert.That(Parse("12387cs831"), Is.EqualTo(0)); // invalid
            Assert.That(Parse("0"), Is.EqualTo(0));
            Assert.That(Parse("-12938723"), Is.EqualTo(-12938723));
            Assert.That(Parse("3948222"), Is.EqualTo(3948222));
            Assert.That(Parse(int.MinValue.ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(int.MinValue));
            Assert.That(Parse(int.MaxValue.ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(int.MaxValue));
            Assert.That(Parse((int.MinValue - 1L).ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(0));
            Assert.That(Parse((int.MaxValue + 1L).ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(0));
        }

        [Test]
        public void ParseRandomInt32()
        {
            var random = TestContext.CurrentContext.Random;
            for (var i = 0; i < 1000; i++)
            {
                var value = random.Next(int.MinValue, int.MaxValue);
                Assert.That(value, Is.EqualTo(JNumber.ParseInt32(value.ToString(NumberFormatInfo.InvariantInfo))));
            }
        }

        [Test]
        public void ParseInt64()
        {
            static long Parse(string s) => JNumber.ParseInt64(s);

            Assert.That(Parse("10000"), Is.EqualTo(10000L));
            Assert.That(Parse("12387cs831"), Is.EqualTo(0L)); // invalid
            Assert.That(Parse("0"), Is.EqualTo(0L));
            Assert.That(Parse("-12938723"), Is.EqualTo(-12938723L));
            Assert.That(Parse("3948222"), Is.EqualTo(3948222L));
            Assert.That(Parse(long.MinValue.ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(long.MinValue));
            Assert.That(Parse(long.MaxValue.ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(long.MaxValue));
            Assert.That(Parse((int.MinValue - 1L).ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(int.MinValue - 1L));
            Assert.That(Parse((int.MaxValue + 1L).ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(int.MaxValue + 1L));
        }

        [Test]
        public void ParseRandomInt64()
        {
            var random = TestContext.CurrentContext.Random;
            for (var i = 0; i < 1000; i++)
            {
                var value =
                    ((long)random.Next(int.MinValue, int.MaxValue) << 32) |
                    ((long)random.Next(int.MinValue, int.MaxValue));
                Assert.That(value, Is.EqualTo(JNumber.ParseInt64(value.ToString(NumberFormatInfo.InvariantInfo))));
            }
        }

        [Test]
        public void ParseSingle()
        {
            static float Parse(string s) => JNumber.ParseSingle(s);

            Assert.That(Parse("10000"), Is.EqualTo(10000.0f));
            Assert.That(Parse("0"), Is.EqualTo(0.0f));
            Assert.That(Parse("-1293.8723"), Is.EqualTo(-1293.8723f));
            Assert.That(Parse("3948.222"), Is.EqualTo(3948.222f));
            Assert.That(Parse("123.45e+6"), Is.EqualTo(123.45e+6f));
            Assert.That(Parse("123.45e6"), Is.EqualTo(123.45e6f));
            Assert.That(Parse("1E3"), Is.EqualTo(1E3f));
            Assert.That(Parse("1e-3"), Is.EqualTo(1e-3f));

            const float Epsilon = 0.00001f;
            Assert.That(Parse("7.96153846"), Is.EqualTo(7.96153846f).Within(Epsilon));
            Assert.That(Parse("7.961538461"), Is.EqualTo(7.961538461f).Within(Epsilon));
            Assert.That(Parse("7.9615384615"), Is.EqualTo(7.9615384615f).Within(Epsilon));
            Assert.That(Parse("7.96153846153"), Is.EqualTo(7.96153846153f).Within(Epsilon));
            Assert.That(Parse("7.961538461538"), Is.EqualTo(7.961538461538f).Within(Epsilon));
            Assert.That(Parse("7.9615384615384635"), Is.EqualTo(7.9615384615384635f).Within(Epsilon));
            Assert.That(JNumber.ParseSingle("{\"hello\": 7.9615384615384635}", 10), Is.EqualTo(7.9615384615384635f).Within(Epsilon));
            Assert.That(JNumber.ParseSingle("{\"hello\": 7.9615384615384635, \"world\": 1}", 10), Is.EqualTo(7.9615384615384635f).Within(Epsilon));
        }

        [TestCase("10000", ExpectedResult = 10000.0)]
        [TestCase("2147483647", ExpectedResult = 2147483647.0)]
        [TestCase("0", ExpectedResult = 0.0)]
        [TestCase("-1293.8723", ExpectedResult = -1293.8723)]
        [TestCase("3948.222", ExpectedResult = 3948.222)]
        [TestCase("123.45e+6", ExpectedResult = 123.45e+6)]
        [TestCase("123.45e6", ExpectedResult = 123.45e6)]
        [TestCase("1E3", ExpectedResult = 1E3)]
        public double ParseDouble(string input) => JNumber.ParseDouble(input);

        [Test]
        public void ParseRandomDouble()
        {
            var min = -10000000.0;
            var max = +10000000.0;

            var specifiers = new string[] { "G", "E", "e" };
            var random = TestContext.CurrentContext.Random;
            for (var i = 0; i < 1000; i++)
            {
                var value = Math.Round(random.NextDouble() * (max - min) + min, 6);

                Assert.That(JNumber.ParseDouble(value.ToString(NumberFormatInfo.InvariantInfo)), Is.EqualTo(value));
                var valueString = value.ToString(specifiers[random.Next(specifiers.Length)], NumberFormatInfo.InvariantInfo);
                Assert.That(JNumber.ParseDouble(valueString), Is.EqualTo(double.Parse(valueString)).Within(0.000001));
            }
        }
    }
}
