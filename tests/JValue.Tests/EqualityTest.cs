using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class EqualityTest
    {
        [TestMethod]
        public void TestRuntimeValues()
        {
            Assert.AreEqual(JValue.Null, new JValue());
            Assert.AreEqual(JValue.Null, default(JValue));
            Assert.AreEqual(JValue.Null, JValue.Parse("null"));
            Assert.AreEqual(JValue.Null, JValue.Null);

            Assert.AreEqual(JValue.EmptyString, Parse(@""""""));
            Assert.AreEqual(JValue.EmptyArray, Parse(@"[   ]"));
            Assert.AreEqual(JValue.EmptyObject, Parse(@"{   }"));
            Assert.AreEqual(JValue.True, Parse(@"true"));
            Assert.AreNotEqual(JValue.True, Parse(@"false"));
            Assert.AreEqual(JValue.False, Parse(@"false"));
            Assert.AreNotEqual(JValue.False, Parse(@"true"));
        }

        [DataTestMethod]
        [DataRow("마린", @"\ub9c8\ub9b0")]
        public void TestEscapeString(string unescaped, string escaped)
            => Assert.AreEqual(JsonEncoding.HexEscape(unescaped), JsonEncoding.HexEscape(escaped));

        [DataTestMethod]
        [DataRow(123, @"123")]
        public void TestInt32EqualToJson(int value, string json)
            => Assert.AreEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow(123, @"456")]
        [DataRow(123, @"""123""")]
        [DataRow(123, @"true")]
        [DataRow(123, @"false")]
        public void TestInt32NotEqualToJson(int value, string json)
            => Assert.AreNotEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow("마린", @"""\ub9c8\ub9b0""")]
        public void TestStringEqualToJson(string value, string json)
            => Assert.AreEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow("마린A", @"""\ub9c8\ub9b0""")]
        [DataRow("마린", @"""\ub9c8\ub9b0A""")]
        public void TestStringNotEqualToJson(string value, string json)
            => Assert.AreNotEqual(new JValue(value), Parse(json));

        [DataTestMethod]
        [DataRow(@"[1,2,3]", @" [ 1, 2, 3 ]")]
        [DataRow(@"{""x"": 10, ""y"": [1,2]}", @"  { ""x"":    10,  ""y""   :  [1  ,   2]     }  ")]
        public void TestJsonEqualToJson(string a, string b)
             => Assert.AreEqual(Parse(a), Parse(b));

        [DataTestMethod]
        [DataRow(@"[1,2,3]", @"123")]
        [DataRow(@"{""x"": 10, ""y"": [1,2]}", @"{""x"": 10, ""y"": 20}")]
        public void TestJsonNotEqualToJson(string a, string b)
            => Assert.AreNotEqual(Parse(a), Parse(b));

        [TestMethod]
        public void TestNestedArrayEquality()
        {
            Assert.AreEqual(Parse("[1,2,3,4]"), Parse("[[1,2,3,4]]")[0]);
        }

        private static JValue Parse(string source)
        {
            var json = JValue.Parse(source);
            Assert.AreEqual(json, json);
            Assert.AreEqual(json, JValue.Parse(json.Serialize(0)));
            Assert.AreEqual(json, JValue.Parse(json.Serialize(2)));
            return json;
        }
    }
}
