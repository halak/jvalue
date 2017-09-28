using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class JsonSerializeTest
    {
        [TestMethod]
        public void TestBasicValueSerializing()
        {
            Assert.AreEqual(new JValue(true).ToString(), "true");
            Assert.AreEqual(new JValue(false).ToString(), "false");
            Assert.AreEqual(new JValue(10).Type, JValue.TypeCode.Number);
            Assert.AreEqual(new JValue(10).AsInt(), 10);
            Assert.AreEqual(new JValue("Hello\nWorld").AsString(), "Hello\nWorld");

        }

        [TestMethod]
        public void TestArrayBuilder()
        {
            var simpleArray = new JValue.ArrayBuilder()
                .Push(10)
                .Push(20)
                .Push(30)
                .Build();

            Assert.AreEqual(simpleArray.ToString(), @"[10,20,30]");
            Assert.AreEqual(simpleArray.Serialize(0), @"[10,20,30]");
            Assert.AreEqual(simpleArray.Serialize(2), @"[10, 20, 30]");
        }

        [TestMethod]
        public void TestObjectBuilder()
        {
            var simpleObject = new JValue.ObjectBuilder()
                .Put("id", 10)
                .Put("name", "John")
                .Put("age", 29)
                .Put("alive", true)
                .Build();

            Assert.AreEqual(simpleObject.ToString(), @"{""id"":10,""name"":""John"",""age"":29,""alive"":true}");
            Assert.AreEqual(simpleObject.Serialize(2), @"{""id"": 10, ""name"": ""John"", ""age"": 29, ""alive"": true}");

            var complexObject = new JValue.ObjectBuilder()
                .Put("name", "Mike")
                .PutArray("jobs", jobs => jobs.Push("chef").Push("programmer").Push("designer"))
                .Build();

            Assert.AreEqual(
                complexObject.ToString(),
                @"{""name"":""Mike"",""jobs"":[""chef"",""programmer"",""designer""]}");
        }

        [TestMethod]
        public void TestEuropeCultureEnvironment()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var testCulture = new CultureInfo("de-DE");
            CultureInfo.CurrentCulture = testCulture;

            try
            {
                Assert.AreEqual((1234.5678).ToString(), "1234,5678");  // test culture change

                Assert.AreEqual(new JValue(1234.5678).ToString(), "1234.5678");
                Assert.AreEqual((string)new JValue.ObjectBuilder().Put("test", 1234.5678).Build()["test"], "1234.5678");
                Assert.AreEqual((string)new JValue.ArrayBuilder().Push(1234.5678).Build()[0], "1234.5678");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}
