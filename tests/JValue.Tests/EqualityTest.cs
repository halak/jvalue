using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class EqualityTest
    {
        [TestMethod]
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

        [DataTestMethod]
        [DataRow("마린", @"\ub9c8\ub9b0")]
        public void TestEscapeString(string unescaped, string escaped)
            => AssertAreEqual(JsonEncoding.HexEscape(unescaped), JsonEncoding.HexEscape(escaped));

        [DataTestMethod]
        [DataRow(123, @"123")]
        public void TestInt32EqualToJson(int value, string json)
            => AssertAreEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow(123, @"456")]
        [DataRow(123, @"""123""")]
        [DataRow(123, @"true")]
        [DataRow(123, @"false")]
        public void TestInt32NotEqualToJson(int value, string json)
            => AssertAreNotEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow("마린", @"""\ub9c8\ub9b0""")]
        public void TestStringEqualToJson(string value, string json)
            => AssertAreEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow("마린A", @"""\ub9c8\ub9b0""")]
        [DataRow("마린", @"""\ub9c8\ub9b0A""")]
        public void TestStringNotEqualToJson(string value, string json)
            => AssertAreNotEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow(@"[1,2,3]", @" [ 1, 2, 3 ]")]
        [DataRow(@"{""x"": 10, ""y"": [1,2]}", @"  { ""x"":    10,  ""y""   :  [1  ,   2]     }  ")]
        public void TestJsonEqualToJson(string a, string b)
             => AssertAreEqual(Parse(a), Parse(b));

        [DataTestMethod]
        [DataRow(@"[1,2,3]", @"123")]
        [DataRow(@"{""x"": 10, ""y"": [1,2]}", @"{""x"": 10, ""y"": 20}")]
        public void TestJsonNotEqualToJson(string a, string b)
            => AssertAreNotEqual(Parse(a), Parse(b));

        [TestMethod]
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
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, JValue.Compare(expected, actual));
        }

        private static void AssertAreNotEqual(JValue expected, JValue actual)
        {
            Assert.AreNotEqual(expected, actual);
            Assert.AreNotEqual(0, JValue.Compare(expected, actual));
        }
    }
}
