using NUnit.Framework;

namespace Halak
{
    public class EqualityTest
    {
        [Test]
        public void TestRuntimeValues()
        {
            AssertAreEqual(JValue.Null, new JValue());
            AssertAreEqual(JValue.Null, default(JValue));
            AssertAreEqual(JValue.Null, JValue.Parse("null"));
            AssertAreEqual(JValue.Null, JValue.Null);

            AssertAreEqual(JValue.EmptyString, Parse(@""""""));
            AssertAreEqual(JValue.EmptyArray, Parse(@"[   ]"));
            AssertAreEqual(JValue.EmptyObject, Parse(@"{   }"));
            AssertAreEqual(JValue.True, Parse(@"true"));
            AssertAreNotEqual(JValue.True, Parse(@"false"));
            AssertAreEqual(JValue.False, Parse(@"false"));
            AssertAreNotEqual(JValue.False, Parse(@"true"));
        }

        [TestCase("마린", @"\ub9c8\ub9b0")]
        public void TestEscapeString(string unescaped, string escaped)
            => AssertAreEqual(JsonEncoding.HexEscape(unescaped), JsonEncoding.HexEscape(escaped));

        [TestCase(123, @"123")]
        public void TestInt32EqualToJson(int value, string json)
            => AssertAreEqual(new JValue(value), Parse(json));

        [TestCase(123, @"456")]
        [TestCase(123, @"""123""")]
        [TestCase(123, @"true")]
        [TestCase(123, @"false")]
        public void TestInt32NotEqualToJson(int value, string json)
            => AssertAreNotEqual(new JValue(value), Parse(json));

        [TestCase("마린", @"""\ub9c8\ub9b0""")]
        public void TestStringEqualToJson(string value, string json)
            => AssertAreEqual(new JValue(value), Parse(json));

        [TestCase("마린A", @"""\ub9c8\ub9b0""")]
        [TestCase("마린", @"""\ub9c8\ub9b0A""")]
        public void TestStringNotEqualToJson(string value, string json)
            => AssertAreNotEqual(new JValue(value), Parse(json));

        [TestCase(@"[1,2,3]", @" [ 1, 2, 3 ]")]
        [TestCase(@"{""x"": 10, ""y"": [1,2]}", @"  { ""x"":    10,  ""y""   :  [1  ,   2]     }  ")]
        public void TestJsonEqualToJson(string a, string b)
             => AssertAreEqual(Parse(a), Parse(b));

        [TestCase(@"[1,2,3]", @"123")]
        [TestCase(@"{""x"": 10, ""y"": [1,2]}", @"{""x"": 10, ""y"": 20}")]
        public void TestJsonNotEqualToJson(string a, string b)
            => AssertAreNotEqual(Parse(a), Parse(b));

        [TestCase]
        public void TestNestedArrayEquality()
        {
            AssertAreEqual(Parse("[1,2,3,4]"), Parse("[[1,2,3,4]]")[0]);
        }

        private static JValue Parse(string source)
        {
            var json = JValue.Parse(source);
            AssertAreEqual(json, json);
            AssertAreEqual(json, JValue.Parse(json.Serialize(0)));
            AssertAreEqual(json, JValue.Parse(json.Serialize(2)));
            return json;
        }

        private static void AssertAreEqual(JValue expected, JValue actual)
        {
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(JValue.Compare(expected, actual), Is.EqualTo(0));
        }

        private static void AssertAreNotEqual(JValue expected, JValue actual)
        {
            Assert.That(actual, Is.Not.EqualTo(expected));
            Assert.That(JValue.Compare(expected, actual), Is.Not.EqualTo(0));
        }
    }
}
