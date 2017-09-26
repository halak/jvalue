using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class JsonSerializeTest
    {
        [TestMethod]
        public void TestBasicValueSerializing()
        {
            Assert.AreEqual(new JValue(10).Type, JValue.TypeCode.Number);
            Assert.AreEqual(new JValue(10).AsInt(), 10);
            Assert.AreEqual(new JValue("Hello\nWorld").AsString(), "Hello\nWorld");
        }
    }
}
