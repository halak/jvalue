using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class JNumberTest
    {
        [TestMethod]
        public void Equality()
        {
            Assert.AreEqual(JNumber.NaN, JNumber.NaN);
            Assert.AreNotEqual(JNumber.NaN, new JNumber(1234));
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
