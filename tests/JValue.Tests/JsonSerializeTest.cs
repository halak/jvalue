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
            Assert.AreEqual("true", new JValue(true).ToString());
            Assert.AreEqual("false", new JValue(false).ToString());
            Assert.AreEqual(JValue.TypeCode.Number, new JValue(10).Type);
            Assert.AreEqual(10, new JValue(10).ToInt32());
            Assert.AreEqual("Hello\nWorld", new JValue("Hello\nWorld").ToUnescapedString());
        }

        [TestMethod]
        public void TestArrayBuilder()
        {
            var simpleArray = new JValue.ArrayBuilder()
                .Push(10)
                .Push(20)
                .Push(30)
                .Build();

            Assert.AreEqual(@"[10,20,30]", simpleArray.ToString());
            Assert.AreEqual(@"[10,20,30]", simpleArray.Serialize(0));
            Assert.AreEqual(@"[10, 20, 30]", simpleArray.Serialize(2));
        }

        [TestMethod]
        public void TestObjectBuilder()
        {
            var simpleObject = new JValue.ObjectBuilder()
                .Put("id", 10)
                .Put("name", "John")
                .Put("age", 29)
                .Put("alive", true)
                .PutNull("friends")
                .Build();

            Assert.AreEqual(@"{""id"":10,""name"":""John"",""age"":29,""alive"":true,""friends"":null}", simpleObject.ToString());
            Assert.AreEqual(@"{""id"": 10, ""name"": ""John"", ""age"": 29, ""alive"": true, ""friends"": null}", simpleObject.Serialize(2));

            var complexObject = new JValue.ObjectBuilder()
                .Put("name", "Mike")
                .PutArray("jobs", jobs => jobs.Push("chef").Push("programmer").Push("designer"))
                .PutNull("children")
                .Build();

            Assert.AreEqual(
                @"{""name"":""Mike"",""jobs"":[""chef"",""programmer"",""designer""],""children"":null}",
                complexObject.ToString());
        }

        [TestMethod]
        public void TestEuropeCultureEnvironment()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var testCulture = new CultureInfo("de-DE");
            CultureInfo.CurrentCulture = testCulture;

            try
            {
                Assert.AreEqual("1234,5678", (1234.5678).ToString());  // test culture change

                Assert.AreEqual("1234.5678", new JValue(1234.5678).ToString());
                Assert.AreEqual("1234.5678", (string)new JValue.ObjectBuilder().Put("test", 1234.5678).Build()["test"]);
                Assert.AreEqual("1234.5678", (string)new JValue.ArrayBuilder().Push(1234.5678).Build()[0]);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void TestStringBuilderExtension()
        {
            foreach (var value in new[] { int.MinValue, int.MaxValue, 0, -1234, 5677, 23472634, -12391823, 23487621 })
            {
                var expected = value.ToString(CultureInfo.InvariantCulture);
                var actual = new JValue.ArrayBuilder().Push(value).Build()[0].ToString();
                Assert.AreEqual(expected, actual);
            }

            foreach (var value in new[] { long.MinValue, long.MaxValue, 0L, -1234L, 239482734L, 2359237498237492837L, -3871623123L })
            {
                var expected = value.ToString(CultureInfo.InvariantCulture);
                var actual = new JValue.ArrayBuilder().Push(value).Build()[0].ToString();
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
