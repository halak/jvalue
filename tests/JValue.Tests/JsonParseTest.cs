using System.Linq;
using NUnit.Framework;

namespace Halak
{
    public class JsonParseTest
    {
        [Test]
        public void TestBasicValueParsing()
        {
            Assert.That((bool)JValue.Parse("true"), Is.True);
            Assert.That((bool)JValue.Parse("false"), Is.False);
            Assert.That(JValue.Parse("10").ToInt32(), Is.EqualTo(10));
        }

        [Test]
        public void TestInvalidValueParsing()
        {
            Assert.That(new JValue("environment").ToInt32(-1), Is.EqualTo(-1));
        }

        [Test]
        public void TestParsedValueType()
        {
            Assert.That(JValue.Parse("true").Type, Is.EqualTo(JValue.TypeCode.Boolean));
            Assert.That(JValue.Parse("false").Type, Is.EqualTo(JValue.TypeCode.Boolean));
            Assert.That(JValue.Parse("10").Type, Is.EqualTo(JValue.TypeCode.Number));
            Assert.That(JValue.Parse("100").Type, Is.EqualTo(JValue.TypeCode.Number));
            Assert.That(JValue.Parse("10.0").Type, Is.EqualTo(JValue.TypeCode.Number));
            Assert.That(JValue.Parse("  50.0     ").Type, Is.EqualTo(JValue.TypeCode.Number));
            Assert.That(JValue.Parse("\"Hello\"").Type, Is.EqualTo(JValue.TypeCode.String));
            Assert.That(JValue.Parse("\"World Hello\"").Type, Is.EqualTo(JValue.TypeCode.String));
            Assert.That(JValue.Parse("").Type, Is.EqualTo(JValue.TypeCode.Null));
            Assert.That(JValue.Parse("null").Type, Is.EqualTo(JValue.TypeCode.Null));
            Assert.That(new JValue().Type, Is.EqualTo(JValue.TypeCode.Null));
        }

        [Test]
        public void TestBasicArrayParsing()
        {
            var array = JValue.Parse(@"   [10,  20    ,  [10  ,30,40 ]     ,30 ,""Hello""  , ""World"", 1]");
            Assert.That(array.Type, Is.EqualTo(JValue.TypeCode.Array));

            var elements = array.Array().ToArray();
            Assert.That((int)elements[0], Is.EqualTo(10));
            Assert.That((int)elements[1], Is.EqualTo(20));
            Assert.That(elements[2].Type, Is.EqualTo(JValue.TypeCode.Array));
            Assert.That((int)elements[3], Is.EqualTo(30));
            Assert.That((string)elements[4], Is.EqualTo("Hello"));
            Assert.That((string)elements[5], Is.EqualTo("World"));
            Assert.That((int)elements[6], Is.EqualTo(1));

            var subElements = array[2].ToArray();
            Assert.That((int)subElements[0], Is.EqualTo(10));
            Assert.That((int)subElements[1], Is.EqualTo(30));
            Assert.That((int)subElements[2], Is.EqualTo(40));
        }

        [Test]
        public void TestFlatObjectParsing()
        {
            var person = JValue.Parse(@"{
                ""first_name"": ""Mario"",
                ""last_name"":  ""Kim"",
                ""age"": 30,
                ""job"": ""Programmer""
            }");

            Assert.That((string)person["first_name"], Is.EqualTo("Mario"));
            Assert.That((string)person["last_name"], Is.EqualTo("Kim"));
            Assert.That((int)person["age"], Is.EqualTo(30));
            Assert.That((string)person["job"], Is.EqualTo("Programmer"));
        }

        [Test]
        public void TestComplexObjectParsing()
        {
            var book = JValue.Parse(@"{
                ""name"": ""Json guide"",
                ""pages"": 400,
                ""tags"": [""computer"", ""data-interchange format"", ""easy""],
                ""price"": {""usd"":0.99, ""krw"":1000}
            }");

            Assert.That(book["name"].ToUnescapedString(), Is.EqualTo("Json guide"));
            Assert.That((int)book["pages"], Is.EqualTo(400));

            Assert.That((string)book["tags"][0], Is.EqualTo("computer"));
            Assert.That((string)book["tags"][1], Is.EqualTo("data-interchange format"));
            Assert.That((string)book["tags"][2], Is.EqualTo("easy"));
            Assert.That(book["tags"][3].Type, Is.EqualTo(JValue.TypeCode.Null));
            Assert.That((string)book["tags"][100], Is.EqualTo(""));

            Assert.That((double)book["price"]["usd"], Is.EqualTo(0.99));
            Assert.That((int)book["price"]["krw"], Is.EqualTo(1000));
            Assert.That(book["price"]["krw"].ToDouble(), Is.EqualTo(1000.0));

            var app = JValue.Parse(@"{
                ""nameRevision"": ""1.0"",
                ""name"": ""hello json""
            }");

            Assert.That((string)app["nameRevision"], Is.EqualTo("1.0"));
            Assert.That((string)app["name"], Is.EqualTo("hello json"));
            Assert.That(app["n"], Is.EqualTo(JValue.Null));
            Assert.That(app["nameNo"], Is.EqualTo(JValue.Null));
        }

        [Test]
        public void TestEscapedStringParsing()
        {
            Assert.That(JValue.Parse(@"""\ub9c8\ub9b0""").ToUnescapedString(), Is.EqualTo("마린"));

            var fileTable = JValue.Parse("{\"C:\\\\hello\\\\world.txt\": \"awesome\nworld\"}");
            Assert.That(fileTable["C:\\hello\\world.txt"].ToUnescapedString(), Is.EqualTo("awesome\nworld"));

            Assert.That(new JValue("\u0000\u001F\u0015").ToString(), Is.EqualTo("\"\\u0000\\u001F\\u0015\""));
            Assert.That(new JValue("\u0000\u001F\u0015").ToUnescapedString(), Is.EqualTo("\u0000\u001F\u0015"));
            Assert.That(new JValue("hello\nworld").ToString(), Is.EqualTo("\"hello\\nworld\""));
        }
    }
}