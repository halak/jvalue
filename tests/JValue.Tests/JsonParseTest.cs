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
            Assert.AreEqual(JValue.TypeCode.Boolean, JValue.Parse("true").Type);
            Assert.AreEqual(JValue.TypeCode.Boolean, JValue.Parse("false").Type);
            Assert.AreEqual(JValue.TypeCode.Number, JValue.Parse("10").Type);
            Assert.AreEqual(JValue.TypeCode.Number, JValue.Parse("100").Type);
            Assert.AreEqual(JValue.TypeCode.Number, JValue.Parse("10.0").Type);
            Assert.AreEqual(JValue.TypeCode.Number, JValue.Parse("  50.0     ").Type);
            Assert.AreEqual(JValue.TypeCode.String, JValue.Parse("\"Hello\"").Type);
            Assert.AreEqual(JValue.TypeCode.String, JValue.Parse("\"World Hello\"").Type);
            Assert.AreEqual(JValue.TypeCode.Null, JValue.Parse("").Type);
            Assert.AreEqual(JValue.TypeCode.Null, JValue.Parse("null").Type);
            Assert.AreEqual(JValue.TypeCode.Null, new JValue().Type);
        }

        [TestMethod]
        public void TestBasicArrayParsing()
        {
            var array = JValue.Parse(@"   [10,  20    ,  [10  ,30,40 ]     ,30 ,""Hello""  , ""World"", 1]");
            Assert.AreEqual(array.Type, JValue.TypeCode.Array);

            var elements = array.Array().ToArray();
            Assert.AreEqual(10, (int)elements[0]);
            Assert.AreEqual(20, (int)elements[1]);
            Assert.AreEqual(JValue.TypeCode.Array, elements[2].Type);
            Assert.AreEqual(30, (int)elements[3]);
            Assert.AreEqual("Hello", (string)elements[4]);
            Assert.AreEqual("World", (string)elements[5]);
            Assert.AreEqual(1, (int)elements[6]);

            var subElements = array[2].AsArray();
            Assert.AreEqual(10, (int)subElements[0]);
            Assert.AreEqual(30, (int)subElements[1]);
            Assert.AreEqual(40, (int)subElements[2]);
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

            Assert.AreEqual("Mario", (string)person["first_name"]);
            Assert.AreEqual("Kim", (string)person["last_name"]);
            Assert.AreEqual(30, (int)person["age"]);
            Assert.AreEqual("Programmer", (string)person["job"]);
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

            Assert.AreEqual("Json guide", book["name"].AsString());
            Assert.AreEqual(400, (int)book["pages"]);

            Assert.AreEqual("computer", (string)book["tags"][0]);
            Assert.AreEqual("data-interchange format", (string)book["tags"][1]);
            Assert.AreEqual("easy", (string)book["tags"][2]);
            Assert.AreEqual(JValue.TypeCode.Null, book["tags"][3].Type);
            Assert.AreEqual("", (string)book["tags"][100]);

            Assert.AreEqual(0.99, (double)book["price"]["usd"]);
            Assert.AreEqual(1000, (int)book["price"]["krw"]);

            var app = JValue.Parse(@"{
                ""nameRevision"": ""1.0"",
                ""name"": ""hello json""
            }");

            Assert.AreEqual("1.0", (string)app["nameRevision"]);
            Assert.AreEqual("hello json", (string)app["name"]);
            Assert.AreEqual(JValue.Null, app["n"]);
            Assert.AreEqual(JValue.Null, app["nameNo"]);
        }

        [TestMethod]
        public void TestEscapedStringParsing()
        {
            Assert.AreEqual("¸¶¸°", JValue.Parse(@"""\ub9c8\ub9b0""").AsString());

            var fileTable = JValue.Parse("{\"C:\\\\hello\\\\world.txt\": \"awesome\nworld\"}");
            Assert.AreEqual("awesome\nworld", fileTable["C:\\hello\\world.txt"].AsString());
        }

        [TestMethod]
        public void TestCommentedJsonParsing()
        {
            var commented = JValue.Parse(@"
            {
                ""Life"": 100,  // Life is high!
                /*
                    Comment isn't JSON stadnard!
                */
                // ""Mana"":/* test weired location comment */ 20,
                ""Mana"":/* test weired location comment */ 10,
                ""Items"": [
                    ""Sword"",  // Long sword
                    ""Shield""  // Small shield
                ]
            }");

            Assert.AreEqual(100, commented["Life"].AsInt());
            Assert.AreEqual(10, commented["Mana"].AsInt());
            Assert.AreEqual("Sword", commented["Items"][0].AsString());
            Assert.AreEqual("Shield", commented["Items"][1].AsString());
        }
    }
}
