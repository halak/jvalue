using System.Linq;
using NUnit.Framework;

namespace Halak
{
    public class ComparisonTest
    {
        [Test]
        public void TestRuntimeValues()
        {
            AssertOrdering(JValue.Null, JValue.False, JValue.True, JValue.EmptyString, JValue.EmptyArray, JValue.EmptyObject);
        }

        [TestCase(@"""a""", @"""b""")]
        [TestCase(@"""ab""", @"""abc""")]
        [TestCase(@"[1,2]", @"[2,3]")]
        [TestCase(@"[1,2]", @"[1,2,3]")]
        public void TestOrdering(params string[] values)
            => AssertOrdering(values.Select((value) => Parse(value)).ToArray());

        private static JValue Parse(string source)
        {
            var json = JValue.Parse(source);
            Assert.That(JValue.Compare(json, json), Is.EqualTo(0));
            Assert.That(JValue.Compare(json, JValue.Parse(json.Serialize(0))), Is.EqualTo(0));
            Assert.That(JValue.Compare(json, JValue.Parse(json.Serialize(2))), Is.EqualTo(0));
            return json;
        }

        private static void AssertOrdering(params JValue[] values)
        {
            for (var i = 0; i < values.Length - 1; i++)
            {
                for (var k = i + 1; k < values.Length; k++)
                {
                    Assert.That(values[i] < values[k], Is.True, $"'{values[i]} < {values[k]}' is false.");
                    Assert.That(values[i], Is.LessThan(values[k]), $"'{values[i]} < {values[k]}' is false.");
                    Assert.That(values[k] > values[i], Is.True, $"'{values[k]} > {values[i]}' is false.");
                    Assert.That(values[k], Is.GreaterThan(values[i]), $"'{values[k]} > {values[i]}' is false.");
                }
            }
        }
    }
}