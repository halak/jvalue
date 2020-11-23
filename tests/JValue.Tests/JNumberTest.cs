using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class JNumberTest
    {
        [TestMethod]
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

        [DataTestMethod]
        [DataRow("-0.000000000000000000000000000000000000000000000000000000000000000000000000000001", "-0", "000000000000000000000000000000000000000000000000000000000000000000000000000001")]
        [DataRow("-0", "-0")]
        [DataRow("-1", "-1")]
        [DataRow("-123", "-123")]
        [DataRow("-123123123123123123123123123123", "-123123123123123123123123123123")]
        [DataRow("-123123e100000", "-123123", null, "100000")]
        [DataRow("-1e+9999", "-1", null, "9999")]
        [DataRow("-237462374673276894279832", "-237462374673276894279832")]
        [DataRow("-237462374673276894279832749832423479823246327846", "-237462374673276894279832749832423479823246327846")]
        [DataRow("0.4e00669999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999969999999006", "0", "4", "00669999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999969999999006")]
        [DataRow("0e+1", "0", null, "1")]
        [DataRow("0e1", "0", null, "1")]
        [DataRow("1.0", "1", "0")]
        [DataRow("1.5e+9999", "1", "5", "9999")]
        [DataRow("100000000000000000000", "100000000000000000000")]
        [DataRow("123.456789", "123", "456789")]
        [DataRow("123.456e-789", "123", "456", "-789")]
        [DataRow("123.456e78", "123", "456", "78")]
        [DataRow("123", "123")]
        [DataRow("123123e100000", "123123", null, "100000")]
        [DataRow("123e-10000000", "123", null, "-10000000")]
        [DataRow("123e45", "123", null, "45")]
        [DataRow("123e65", "123", null, "65")]
        [DataRow("1e-2", "1", null, "-2")]
        [DataRow("1E-2", "1", null, "-2")]
        [DataRow("1e+2", "1", null, "2")]
        [DataRow("1E+2", "1", null, "2")]
        [DataRow("1E22", "1", null, "22")]
        [DataRow("20e1", "20", null, "1")]
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

        [DataTestMethod]
        [DataRow("", 0)]
        [DataRow("123", 0)]
        [DataRow("0.001", 2)]
        [DataRow("0.01", 1)]
        [DataRow("1.0E+2", 1)]
        [DataRow("1.00E+2", 2)]
        [DataRow("1.001E+2", 2)]
        [DataRow("1.0000011123234", 5)]
        public void LeadingZeros(string input, int expectedLeadingZeros)
        {
            Assert.AreEqual(expectedLeadingZeros, JNumber.Parse(input).FractionalPart.LeadingZeros);
        }

        [TestMethod]
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
