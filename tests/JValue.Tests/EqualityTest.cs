using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class EqualityTest
    {
        [TestMethod]
        public void TestNullEquality()
        {
            Assert.AreEqual(new JValue(), JValue.Null);
            Assert.AreEqual(default(JValue), JValue.Null);
            Assert.AreEqual(JValue.Parse("null"), JValue.Null);
            Assert.AreEqual(JValue.Null, JValue.Null);
        }

        [TestMethod]
        public void TestEscapedStringEquality()
        {
            Assert.AreEqual(JValue.Parse(@"""\ub9c8\ub9b0"""), new JValue("마린"));
            Assert.AreNotEqual(JValue.Parse(@"""\ub9c8\ub9b0"""), new JValue("마린A"));
            Assert.AreNotEqual(JValue.Parse(@"""\ub9c8\ub9b0A"""), new JValue("마린"));
        }

        [TestMethod]
        public void TestComplexEquality()
        {
            var a = JValue.Parse("[1,2,3,4]");
            var b = JValue.Parse("[[1,2,3,4]]")[0];

            Assert.AreEqual(a, a);
            Assert.AreNotEqual(a, b);
        }
    }
}
