using System;
using System.Globalization;
using NUnit.Framework;

namespace Halak
{
    public class JsonSerializeTest
    {
        [Test]
        public void TestBasicValueSerializing()
        {
            Assert.That(new JValue(true).ToString(), Is.EqualTo("true"));
            Assert.That(new JValue(false).ToString(), Is.EqualTo("false"));
            Assert.That(new JValue(10).Type, Is.EqualTo(JValue.TypeCode.Number));
            Assert.That(new JValue(10).ToInt32(), Is.EqualTo(10));
            Assert.That(new JValue("Hello\nWorld").ToUnescapedString(), Is.EqualTo("Hello\nWorld"));
        }

        [Test]
        public void TestArrayBuilder()
        {
            var simpleArray = new JsonArrayBuilder(16)
                .Push(10)
                .Push(20)
                .Push(30)
                .Build();

            Assert.That(simpleArray.ToString(), Is.EqualTo(@"[10,20,30]"));
            Assert.That(simpleArray.Serialize(0), Is.EqualTo(@"[10,20,30]"));
            Assert.That(simpleArray.Serialize(2), Is.EqualTo(@"[10, 20, 30]"));
        }

        [Test]
        public void TestObjectBuilder()
        {
            var simpleObject = new JsonObjectBuilder(16)
                .Put("id", 10)
                .Put("name", "John")
                .Put("age", 29)
                .Put("alive", true)
                .PutNull("friends")
                .Build();

            Assert.That(simpleObject.ToString(), Is.EqualTo(@"{""id"":10,""name"":""John"",""age"":29,""alive"":true,""friends"":null}"));
            Assert.That(simpleObject.Serialize(2), Is.EqualTo(@"{""id"": 10, ""name"": ""John"", ""age"": 29, ""alive"": true, ""friends"": null}"));

            var complexObject = new JsonObjectBuilder(64)
                .Put("name", "Mike")
                .PutArray("jobs", jobs => jobs.Push("chef").Push("programmer").Push("designer"))
                .PutNull("children")
                .Build();

            Assert.That(
                complexObject.ToString(), Is.EqualTo(@"{""name"":""Mike"",""jobs"":[""chef"",""programmer"",""designer""],""children"":null}"));
        }

        [Test]
        public void TestObjectBuilderWithoutChaining()
        {
            var builder = new JsonObjectBuilder(1024);
            PutJobs(builder);
            builder.Put("name", "Mike");
            builder.PutNull("children");
            var complexObject = builder.Build();
            Assert.That(
                complexObject.ToString(), Is.EqualTo(@"{""jobs"":[""chef"",""programmer"",""designer""],""name"":""Mike"",""children"":null}"));

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

        [Test]
        public void TestDeutschlandCultureEnvironment()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var testCulture = new CultureInfo("de-DE");
            CultureInfo.CurrentCulture = testCulture;

            try
            {
                Assert.That((1234.5678).ToString(), Is.EqualTo("1234,5678")); // Test changed culture.

                Assert.That(new JValue(1234.5678).ToString(), Is.EqualTo("1234.5678"));
                Assert.That((string)new JsonObjectBuilder(16).Put("test", 1234.5678).Build()["test"], Is.EqualTo("1234.5678"));
                Assert.That((string)new JsonArrayBuilder(16).Push(1234.5678).Build()[0], Is.EqualTo("1234.5678"));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Test]
        public void TestPutIf()
        {
            AssertAreEqual(x => x.PutNull("x"), x => x.PutNullIf(true, "x"));
            AssertAreEqual(x => x.Put("x", true), x => x.PutIf(true, "x", true));
            AssertAreEqual(x => x.Put("x", 123), x => x.PutIf(true, "x", 123));
            AssertAreEqual(x => x.Put("x", 123U), x => x.PutIf(true, "x", 123U));
            AssertAreEqual(x => x.Put("x", 123L), x => x.PutIf(true, "x", 123L));
            AssertAreEqual(x => x.Put("x", 123UL), x => x.PutIf(true, "x", 123UL));
            AssertAreEqual(x => x.Put("x", 123.0f), x => x.PutIf(true, "x", 123.0f));
            AssertAreEqual(x => x.Put("x", 123.0), x => x.PutIf(true, "x", 123.0));
            AssertAreEqual(x => x.Put("x", 123.0m), x => x.PutIf(true, "x", 123.0m));
            AssertAreEqual(x => x.PutArray("x", new[] { 1, 2, 3 }), x => x.PutArrayIf(true, "x", new[] { 1, 2, 3 }));
            AssertAreEqual(x => x.PutArray("x", new[] { "A", "B", "C" }), x => x.PutArrayIf(true, "x", new[] { "A", "B", "C" }));

            AssertAreEqual(x => x, x => x.PutNullIf(false, "x"));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", true));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123U));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123L));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123UL));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123.0f));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123.0));
            AssertAreEqual(x => x, x => x.PutIf(false, "x", 123.0m));
            AssertAreEqual(x => x, x => x.PutArrayIf(false, "x", new[] { 1, 2, 3 }));
            AssertAreEqual(x => x, x => x.PutArrayIf(false, "x", new[] { "A", "B", "C" }));

            static void AssertAreEqual(Func<JsonObjectBuilder, JsonObjectBuilder> build, Func<JsonObjectBuilder, JsonObjectBuilder> buildConditional)
                => Assert.That(buildConditional(new JsonObjectBuilder(32)).Build(), Is.EqualTo(build(new JsonObjectBuilder(32)).Build()));
        }

        [Test]
        public void TestPushIf()
        {
            AssertAreEqual(x => x.PushNull(), x => x.PushNullIf(true));
            AssertAreEqual(x => x.Push(true), x => x.PushIf(true, true));
            AssertAreEqual(x => x.Push(123), x => x.PushIf(true, 123));
            AssertAreEqual(x => x.Push(123U), x => x.PushIf(true, 123U));
            AssertAreEqual(x => x.Push(123L), x => x.PushIf(true, 123L));
            AssertAreEqual(x => x.Push(123UL), x => x.PushIf(true, 123UL));
            AssertAreEqual(x => x.Push(123.0f), x => x.PushIf(true, 123.0f));
            AssertAreEqual(x => x.Push(123.0), x => x.PushIf(true, 123.0));
            AssertAreEqual(x => x.Push(123.0m), x => x.PushIf(true, 123.0m));
            AssertAreEqual(x => x.PushArray(new[] { 1, 2, 3 }), x => x.PushArrayIf(true, new[] { 1, 2, 3 }));
            AssertAreEqual(x => x.PushArray(new[] { "A", "B", "C" }), x => x.PushArrayIf(true, new[] { "A", "B", "C" }));

            AssertAreEqual(x => x, x => x.PushNullIf(false));
            AssertAreEqual(x => x, x => x.PushIf(false, true));
            AssertAreEqual(x => x, x => x.PushIf(false, 123));
            AssertAreEqual(x => x, x => x.PushIf(false, 123U));
            AssertAreEqual(x => x, x => x.PushIf(false, 123L));
            AssertAreEqual(x => x, x => x.PushIf(false, 123UL));
            AssertAreEqual(x => x, x => x.PushIf(false, 123.0f));
            AssertAreEqual(x => x, x => x.PushIf(false, 123.0));
            AssertAreEqual(x => x, x => x.PushIf(false, 123.0m));
            AssertAreEqual(x => x, x => x.PushArrayIf(false, new[] { 1, 2, 3 }));
            AssertAreEqual(x => x, x => x.PushArrayIf(false, new[] { "A", "B", "C" }));

            static void AssertAreEqual(Func<JsonArrayBuilder, JsonArrayBuilder> build, Func<JsonArrayBuilder, JsonArrayBuilder> buildConditional)
                => Assert.That(buildConditional(new JsonArrayBuilder(32)).Build(), Is.EqualTo(build(new JsonArrayBuilder(32)).Build()));
        }

        [Test]
        public void TestStringBuilderExtension()
        {
            foreach (var value in new[] { int.MinValue, int.MaxValue, 0, -1234, 5677, 23472634, -12391823, 23487621 })
            {
                var expected = value.ToString(NumberFormatInfo.InvariantInfo);
                var actual = new JsonArrayBuilder(32).Push(value).Build()[0].ToString();
                Assert.That(actual, Is.EqualTo(expected));
            }

            foreach (var value in new[] { uint.MinValue, uint.MaxValue, 1234U, 5677U, 23472634U, 12391823U, 23487621U })
            {
                var expected = value.ToString(NumberFormatInfo.InvariantInfo);
                var actual = new JsonArrayBuilder(32).Push(value).Build()[0].ToString();
                Assert.That(actual, Is.EqualTo(expected));
            }

            foreach (var value in new[] { long.MinValue, long.MaxValue, 0L, -1234L, 239482734L, 2359237498237492837L, -3871623123L })
            {
                var expected = value.ToString(NumberFormatInfo.InvariantInfo);
                var actual = new JsonArrayBuilder(32).Push(value).Build()[0].ToString();
                Assert.That(actual, Is.EqualTo(expected));
            }

            foreach (var value in new[] { ulong.MinValue, ulong.MaxValue, 1234UL, 239482734UL, 2359237498237492837UL, 3871623123UL })
            {
                var expected = value.ToString(NumberFormatInfo.InvariantInfo);
                var actual = new JsonArrayBuilder(32).Push(value).Build()[0].ToString();
                Assert.That(actual, Is.EqualTo(expected));
            }
        }
    }
}
