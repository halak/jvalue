using System;
using NUnit.Framework;

namespace Halak
{
    public class JNumberTest
    {
        [Test]
        public void Constants()
        {
            Assert.AreEqual(JNumber.NaN, JNumber.NaN);
            Assert.IsTrue(default(JNumber).IsNaN);
            Assert.IsTrue(JNumber.NaN.IsNaN);
            Assert.IsFalse(JNumber.NaN.IsPositive);
            Assert.IsFalse(JNumber.NaN.IsNegative);
            Assert.IsFalse(JNumber.NaN.HasExponent);
            Assert.IsFalse(JNumber.NaN.HasFractionalPart);
            Assert.AreNotEqual(JNumber.NaN, new JNumber(1234));
            Assert.IsFalse(JNumber.Zero.IsNaN);
            Assert.IsTrue(JNumber.One.IsPositive);
            Assert.AreEqual(JNumber.One, JNumber.One);
            Assert.AreEqual(JNumber.One, new JNumber(1));
            Assert.AreNotEqual(JNumber.One, JNumber.Zero);
        }

        [TestCase("-0.000000000000000000000000000000000000000000000000000000000000000000000000000001", "-0", "000000000000000000000000000000000000000000000000000000000000000000000000000001")]
        [TestCase("-0", "-0")]
        [TestCase("-1", "-1")]
        [TestCase("-123", "-123")]
        [TestCase("-123123123123123123123123123123", "-123123123123123123123123123123")]
        [TestCase("-123123e100000", "-123123", null, "100000")]
        [TestCase("-1e+9999", "-1", null, "9999")]
        [TestCase("-237462374673276894279832", "-237462374673276894279832")]
        [TestCase("-237462374673276894279832749832423479823246327846", "-237462374673276894279832749832423479823246327846")]
        [TestCase("0.4e00669999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999969999999006", "0", "4", "00669999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999969999999006")]
        [TestCase("0e+1", "0", null, "1")]
        [TestCase("0e1", "0", null, "1")]
        [TestCase("1.0", "1")]
        [TestCase("1.5e+9999", "1", "5", "9999")]
        [TestCase("100000000000000000000", "100000000000000000000")]
        [TestCase("123.456789", "123", "456789")]
        [TestCase("123.456e-789", "123", "456", "-789")]
        [TestCase("123.456e78", "123", "456", "78")]
        [TestCase("123", "123")]
        [TestCase("123123e100000", "123123", null, "100000")]
        [TestCase("123e-10000000", "123", null, "-10000000")]
        [TestCase("123e45", "123", null, "45")]
        [TestCase("123e65", "123", null, "65")]
        [TestCase("1e-2", "1", null, "-2")]
        [TestCase("1E-2", "1", null, "-2")]
        [TestCase("1e+2", "1", null, "2")]
        [TestCase("1E+2", "1", null, "2")]
        [TestCase("1E22", "1", null, "22")]
        [TestCase("20e1", "20", null, "1")]
        public void Parse(string input, string expectedIntegerPart, string expectedFractionalPart = null, string expectedExponent = null)
        {
            var number = JNumber.Parse(input);

            if (string.IsNullOrEmpty(expectedIntegerPart) == false)
                Assert.AreEqual(expectedIntegerPart, number.IntegerPart.ToString());
            else
                Assert.IsTrue(number.IntegerPart.IsNaN);

            if (string.IsNullOrEmpty(expectedFractionalPart) == false)
                Assert.AreEqual(expectedFractionalPart, number.FractionalPart.ToString());
            else
                Assert.IsTrue(number.FractionalPart.IsNaN);

            if (string.IsNullOrEmpty(expectedExponent) == false)
                Assert.AreEqual(expectedExponent, number.Exponent.ToString());
            else
                Assert.IsTrue(number.Exponent.IsNaN);
        }

        [TestCase("", "")]
        [TestCase("0", "0")]
        [TestCase("0", "0.0")]
        [TestCase("1", "1")]
        [TestCase("0.1", "0.1")]
        [TestCase("0.1", "0.1000")]
        [TestCase("1", "1.0")]
        [TestCase("1", "1.00")]
        [TestCase("123.456E10", "123.456e10")]
        [TestCase("123.456E10", "123.456e+10")]
        public void Equality(string left, string right)
        {
            Assert.IsTrue(JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)));
            if (left.StartsWith("-", StringComparison.Ordinal) == false)
            {
                left = FormattableString.Invariant($"-{left}");
                right = FormattableString.Invariant($"-{right}");
                Assert.IsTrue(JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)));
            }
        }

        [TestCase("", "0")]
        [TestCase("", "1")]
        [TestCase("", "1.0")]
        [TestCase("0", "1")]
        [TestCase("1", "1.001")]
        [TestCase("1.000", "1.001")]
        [TestCase("123.456E10", "123.456E-10")]
        public void Inequality(string left, string right)
            => Assert.IsFalse(JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)));

        [TestCase("", "1")]
        [TestCase("0", "1")]
        [TestCase("123", "456")]
        [TestCase("123.123", "123.456")]
        [TestCase("1.23", "1.234")]
        [TestCase("1", "-1")]
        [TestCase("-123", "-456")]
        [TestCase("-12", "-456")]
        public void Compare(string less, string greater)
        {
            Assert.IsTrue(JNumber.Compare(JNumber.Parse(less), JNumber.Parse(greater)) < 0);
            Assert.IsTrue(JNumber.Compare(JNumber.Parse(greater), JNumber.Parse(less)) > 0);
        }

        [TestCase("0", "0.0")]
        [TestCase("0.1", "0.1000")]
        [TestCase("1", "1.0")]
        [TestCase("1", "1.00")]
        [TestCase("123.456E10", "123.456e10")]
        [TestCase("123.456E10", "123.456e+10")]
        public void TwoNumber_Are_Equal_But_NotEquivalent(string left, string right)
        {
            Assert.IsTrue(
                JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)) &&
                JNumber.Compare(JNumber.Parse(left), JNumber.Parse(right)) != 0);
        }

        [TestCase("", ExpectedResult = 0)]
        [TestCase("123", ExpectedResult = 0)]
        [TestCase("0.001", ExpectedResult = 2)]
        [TestCase("0.01", ExpectedResult = 1)]
        [TestCase("1.0E+2", ExpectedResult = 0)]
        [TestCase("1.00E+2", ExpectedResult = 0)]
        [TestCase("1.001E+2", ExpectedResult = 2)]
        [TestCase("1.0000011123234", ExpectedResult = 5)]
        public int LeadingZeros(string input)
            => JNumber.Parse(input).FractionalPart.LeadingZeros;

        [Test]
        public void Stringify()
        {
            Assert.AreEqual("0", new JNumber(0).ToString());
            Assert.AreEqual("1234", new JNumber(1234).ToString());
            Assert.AreEqual("-5833", new JNumber(-5833).ToString());

            Assert.AreEqual("-123", new JNumber(-123.456m).IntegerPart.ToString());
            Assert.AreEqual("456", new JNumber(-123.456m).FractionalPart.ToString());
        }
    }
}
