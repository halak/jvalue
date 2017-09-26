using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class JsonParseTest
    {
        [TestMethod]
        public void TestBasicValueParsing()
        {
            Assert.IsTrue(JValue.Parse("true"));
            Assert.IsFalse(JValue.Parse("false"));
            Assert.AreEqual(JValue.Parse("10").AsInt(), 10);
        }

        [TestMethod]
        public void TestParsedValueType()
        {
            Assert.AreEqual(JValue.Parse("true").Type, JValue.TypeCode.Boolean);
            Assert.AreEqual(JValue.Parse("false").Type, JValue.TypeCode.Boolean);
            Assert.AreEqual(JValue.Parse("10").Type, JValue.TypeCode.Number);
            Assert.AreEqual(JValue.Parse("100").Type, JValue.TypeCode.Number);
            Assert.AreEqual(JValue.Parse("10.0").Type, JValue.TypeCode.Number);
            Assert.AreEqual(JValue.Parse("50.0").Type, JValue.TypeCode.Number);
            Assert.AreEqual(JValue.Parse("\"Hello\"").Type, JValue.TypeCode.String);
            Assert.AreEqual(JValue.Parse("\"World Hello\"").Type, JValue.TypeCode.String);
            Assert.AreEqual(JValue.Parse("").Type, JValue.TypeCode.Null);
            Assert.AreEqual(JValue.Parse("null").Type, JValue.TypeCode.Null);
            Assert.AreEqual(new JValue().Type, JValue.TypeCode.Null);
        }

        [TestMethod]
        public void TestBasicArrayParsing()
        {
            var array = JValue.Parse(@"   [10,  20    ,  [10  ,30,40 ]     ,30 ,""Hello""  , ""World"", 1]");
            Assert.AreEqual(array.Type, JValue.TypeCode.Array);

            var elements = array.Array().ToArray();
            Assert.AreEqual((int)elements[0], 10);
            Assert.AreEqual((int)elements[1], 20);
            Assert.AreEqual(elements[2].Type, JValue.TypeCode.Array);
            Assert.AreEqual((int)elements[3], 30);
            Assert.AreEqual((string)elements[4], "Hello");
            Assert.AreEqual((string)elements[5], "World");
            Assert.AreEqual((int)elements[6], 1);

            var subElements = array[2].AsArray();
            Assert.AreEqual((int)subElements[0], 10);
            Assert.AreEqual((int)subElements[1], 30);
            Assert.AreEqual((int)subElements[2], 40);
        }

        [TestMethod]
        public void TestFlatObjectParsing()
        {
            var person = JValue.Parse(@"{
                ""first_name"": ""Mario"",
                ""last_name"":  ""Kim"",
                ""age"": 30,
                ""job"": ""Programmer""
            }");

            Assert.AreEqual((string)person["first_name"], "Mario");
            Assert.AreEqual((string)person["last_name"], "Kim");
            Assert.AreEqual((int)person["age"], 30);
            Assert.AreEqual((string)person["job"], "Programmer");
        }

        [TestMethod]
        public void TestComplexObjectParsing()
        {
            var book = JValue.Parse(@"{
                ""name"": ""Json guide"",
                ""pages"": 400,
                ""tags"": [""computer"", ""data-interchange format"", ""easy""],
                ""price"": {""usd"":0.99, ""krw"":1000}
            }");

            Assert.AreEqual(book["name"].AsString(), "Json guide");
            Assert.AreEqual((int)book["pages"], 400);


            Assert.AreEqual((string)book["tags"][0], "computer");
            Assert.AreEqual((string)book["tags"][1], "data-interchange format");
            Assert.AreEqual((string)book["tags"][2], "easy");
            Assert.AreEqual(book["tags"][3].Type, JValue.TypeCode.Null);
            Assert.AreEqual((string)book["tags"][100], "");

            Assert.AreEqual((double)book["price"]["usd"], 0.99);
            Assert.AreEqual((int)book["price"]["krw"], 1000);
        }

        [TestMethod]
        public void TestEscapedStringParsing()
        {
            Assert.AreEqual(JValue.Parse(@"""\ub9c8\ub9b0""").AsString(), "¸¶¸°");

            var fileTable = JValue.Parse("{\"C:\\\\hello\\\\world.txt\": \"awesome\nworld\"}");
            Assert.AreEqual(fileTable["C:\\hello\\world.txt"].AsString(), "awesome\nworld");
        }
    }
}
