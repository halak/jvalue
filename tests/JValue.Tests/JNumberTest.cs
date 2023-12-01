using System;
using NUnit.Framework;

namespace Halak
{
    public class JNumberTest
    {
        [Test]
        public void Constants()
        {
            Assert.That(JNumber.NaN, Is.EqualTo(JNumber.NaN));
            Assert.That(default(JNumber).IsNaN, Is.True);
            Assert.That(JNumber.NaN.IsNaN, Is.True);
            Assert.That(JNumber.NaN.IsPositive, Is.False);
            Assert.That(JNumber.NaN.IsNegative, Is.False);
            Assert.That(JNumber.NaN.HasExponent, Is.False);
            Assert.That(JNumber.NaN.HasFractionalPart, Is.False);
            Assert.That(new JNumber(1234), Is.Not.EqualTo(JNumber.NaN));
            Assert.That(JNumber.Zero.IsNaN, Is.False);
            Assert.That(JNumber.One.IsPositive, Is.True);
            Assert.That(JNumber.One, Is.EqualTo(JNumber.One));
            Assert.That(new JNumber(1), Is.EqualTo(JNumber.One));
            Assert.That(JNumber.Zero, Is.Not.EqualTo(JNumber.One));
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
                Assert.That(number.IntegerPart.ToString(), Is.EqualTo(expectedIntegerPart));
            else
                Assert.That(number.IntegerPart.IsNaN, Is.True);

            if (string.IsNullOrEmpty(expectedFractionalPart) == false)
                Assert.That(number.FractionalPart.ToString(), Is.EqualTo(expectedFractionalPart));
            else
                Assert.That(number.FractionalPart.IsNaN, Is.True);

            if (string.IsNullOrEmpty(expectedExponent) == false)
                Assert.That(number.Exponent.ToString(), Is.EqualTo(expectedExponent));
            else
                Assert.That(number.Exponent.IsNaN, Is.True);
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
            Assert.That(JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)), Is.True);
            if (left.StartsWith("-", StringComparison.Ordinal) == false)
            {
                left = FormattableString.Invariant($"-{left}");
                right = FormattableString.Invariant($"-{right}");
                Assert.That(JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)), Is.True);
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
            => Assert.That(JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)), Is.False);

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
            Assert.That(JNumber.Compare(JNumber.Parse(less), JNumber.Parse(greater)), Is.LessThan(0));
            Assert.That(JNumber.Compare(JNumber.Parse(greater), JNumber.Parse(less)), Is.GreaterThan(0));
        }

        [TestCase("0", "0.0")]
        [TestCase("0.1", "0.1000")]
        [TestCase("1", "1.0")]
        [TestCase("1", "1.00")]
        [TestCase("123.456E10", "123.456e10")]
        [TestCase("123.456E10", "123.456e+10")]
        public void TwoNumber_Are_Equal_But_NotEquivalent(string left, string right)
        {
            Assert.That(
                JNumber.Equals(JNumber.Parse(left), JNumber.Parse(right)) &&
                JNumber.Compare(JNumber.Parse(left), JNumber.Parse(right)) != 0, Is.True);
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
            Assert.That(new JNumber(0).ToString(), Is.EqualTo("0"));
            Assert.That(new JNumber(1234).ToString(), Is.EqualTo("1234"));
            Assert.That(new JNumber(-5833).ToString(), Is.EqualTo("-5833"));

            Assert.That(new JNumber(-123.456m).IntegerPart.ToString(), Is.EqualTo("-123"));
            Assert.That(new JNumber(-123.456m).FractionalPart.ToString(), Is.EqualTo("456"));
        }
    }
}