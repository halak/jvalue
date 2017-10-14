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
            Assert.AreEqual(JValue.Null, new JValue());
            Assert.AreEqual(JValue.Null, default(JValue));
            Assert.AreEqual(JValue.Null, JValue.Parse("null"));
            Assert.AreEqual(JValue.Null, JValue.Null);
        }

        [TestMethod]
        public void TestEscapedStringEquality()
        {
            Assert.AreEqual(new JValue("마린"), JValue.Parse(@"""\ub9c8\ub9b0"""));
            Assert.AreNotEqual(new JValue("마린A"), JValue.Parse(@"""\ub9c8\ub9b0"""));
            Assert.AreNotEqual(new JValue("마린"), JValue.Parse(@"""\ub9c8\ub9b0A"""));
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
