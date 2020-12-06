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
            var simpleArray = new JsonArrayBuilder(16)
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
            var simpleObject = new JsonObjectBuilder(16)
                .Put("id", 10)
                .Put("name", "John")
                .Put("age", 29)
                .Put("alive", true)
                .PutNull("friends")
                .Build();

            Assert.AreEqual(@"{""id"":10,""name"":""John"",""age"":29,""alive"":true,""friends"":null}", simpleObject.ToString());
            Assert.AreEqual(@"{""id"": 10, ""name"": ""John"", ""age"": 29, ""alive"": true, ""friends"": null}", simpleObject.Serialize(2));

            var complexObject = new JsonObjectBuilder(16)
                .Put("name", "Mike")
                .PutArray("jobs", jobs => jobs.Push("chef").Push("programmer").Push("designer"))
                .PutNull("children")
                .Build();

            Assert.AreEqual(
                @"{""name"":""Mike"",""jobs"":[""chef"",""programmer"",""designer""],""children"":null}",
                complexObject.ToString());
        }

        [TestMethod]
        public void TestObjectBuilderWithoutChaining()
        {
            var builder = new JsonObjectBuilder(1024);
            PutJobs(builder);
            builder.Put("name", "Mike");
            builder.PutNull("children");
            var complexObject = builder.Build();
            Assert.AreEqual(
                @"{""jobs"":[""chef"",""programmer"",""designer""],""name"":""Mike"",""children"":null}",
                complexObject.ToString());

            static JsonObjectBuilder PutJobs(JsonObjectBuilder mainBuilder)
            {
                return mainBuilder.PutArray("jobs", jobs =>
                {
                    jobs.Push("chef");
                    jobs.Push("programmer");
                    jobs.Push("designer");
                });
            }
        }

        [TestMethod]
        public void TestDeutschlandCultureEnvironment()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var testCulture = new CultureInfo("de-DE");
            CultureInfo.CurrentCulture = testCulture;

            try
            {
                Assert.AreEqual("1234,5678", (1234.5678).ToString());  // Test changed culture.

                Assert.AreEqual("1234.5678", new JValue(1234.5678).ToString());
                Assert.AreEqual("1234.5678", (string)new JsonObjectBuilder(16).Put("test", 1234.5678).Build()["test"]);
                Assert.AreEqual("1234.5678", (string)new JsonArrayBuilder(16).Push(1234.5678).Build()[0]);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void TestPutIf()
        {
            AssertAreEqual(x => x.PutNull("x"), x => x.PutNullIf(true, "x"));
            AssertAreEqual(x => x.Put("x", true), x => x.PutIf(true, "x", true));
            AssertAreEqual(x => x.Put("x", 123), x => x.PutIf(true, "x", 123));
            AssertAreEqual(x => x.Put("x", 123L), x => x.PutIf(true, "x", 123L));
            AssertAreEqual(x => x.Put("x", 123.0f), x => x.PutIf(true, "x", 123.0f));
            AssertAreEqual(x => x.Put("x", 123.0), x => x.PutIf(true, "x", 123.0));
            AssertAreEqual(x => x.Put("x", 123.0m), x => x.PutIf(true, "x", 123.0m));
            AssertAreEqual(x => x.PutArray("x", new[] { 1, 2, 3 }), x => x.PutArrayIf(true, "x", new[] { 1, 2, 3 }));
            AssertAreEqual(x => x.PutArray("x", new[] { "A", "B", "C" }), x => x.PutArrayIf(true, "x", new[] { "A", "B", "C" }));

            AssertAreEqual(x => x, x => x.PutNullIf(false, "x"));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", true));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123L));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123.0f));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123.0));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123.0m));
            AssertAreEqual(x => x, x => x.PutArrayIf(false, "x", new[] { 1, 2, 3 }));
            AssertAreEqual(x => x, x => x.PutArrayIf(false, "x", new[] { "A", "B", "C" }));

            void AssertAreEqual(Func<JsonObjectBuilder, JsonObjectBuilder> build, Func<JsonObjectBuilder, JsonObjectBuilder> buildConditional)
                => Assert.AreEqual(build(new JsonObjectBuilder(32)).Build(), buildConditional(new JsonObjectBuilder(32)).Build());
        }

        [TestMethod]
        public void TestPushIf()
        {
            AssertAreEqual(x => x.PushNull(), x => x.PushNullIf(true));
            AssertAreEqual(x => x.Push(true), x => x.PushIf(true, true));
            AssertAreEqual(x => x.Push(123), x => x.PushIf(true, 123));
            AssertAreEqual(x => x.Push(123L), x => x.PushIf(true, 123L));
            AssertAreEqual(x => x.Push(123.0f), x => x.PushIf(true, 123.0f));
            AssertAreEqual(x => x.Push(123.0), x => x.PushIf(true, 123.0));
            AssertAreEqual(x => x.Push(123.0m), x => x.PushIf(true, 123.0m));
            AssertAreEqual(x => x.PushArray(new[] { 1, 2, 3 }), x => x.PushArrayIf(true, new[] { 1, 2, 3 }));
            AssertAreEqual(x => x.PushArray(new[] { "A", "B", "C" }), x => x.PushArrayIf(true, new[] { "A", "B", "C" }));

            AssertAreEqual(x => x, x => x.PushNullIf(false));
            AssertAreEqual(x => x, x => x.PushIf(false, true));
            AssertAreEqual(x => x, x => x.PushIf(false, 123));
            AssertAreEqual(x => x, x => x.PushIf(false, 123L));
            AssertAreEqual(x => x, x => x.PushIf(false, 123.0f));
            AssertAreEqual(x => x, x => x.PushIf(false, 123.0));
            AssertAreEqual(x => x, x => x.PushIf(false, 123.0m));
            AssertAreEqual(x => x, x => x.PushArrayIf(false, new[] { 1, 2, 3 }));
            AssertAreEqual(x => x, x => x.PushArrayIf(false, new[] { "A", "B", "C" }));

            void AssertAreEqual(Func<JsonArrayBuilder, JsonArrayBuilder> build, Func<JsonArrayBuilder, JsonArrayBuilder> buildConditional)
                => Assert.AreEqual(build(new JsonArrayBuilder(32)).Build(), buildConditional(new JsonArrayBuilder(32)).Build());
        }

        [TestMethod]
        public void TestStringBuilderExtension()
        {
            foreach (var value in new[] { int.MinValue, int.MaxValue, 0, -1234, 5677, 23472634, -12391823, 23487621 })
            {
                var expected = value.ToString(CultureInfo.InvariantCulture);
                var actual = new JsonArrayBuilder(32).Push(value).Build()[0].ToString();
                Assert.AreEqual(expected, actual);
            }

            foreach (var value in new[] { long.MinValue, long.MaxValue, 0L, -1234L, 239482734L, 2359237498237492837L, -3871623123L })
            {
                var expected = value.ToString(CultureInfo.InvariantCulture);
                var actual = new JsonArrayBuilder(32).Push(value).Build()[0].ToString();
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
