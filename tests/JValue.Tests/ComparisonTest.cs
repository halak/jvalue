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
            => AssertOrdering(values.Select(value => Parse(value)).ToArray());

        private static JValue Parse(string source)
        {
            var json = JValue.Parse(source);
            Assert.AreEqual(0, JValue.Compare(json, json));
            Assert.AreEqual(0, JValue.Compare(json, JValue.Parse(json.Serialize(0))));
            Assert.AreEqual(0, JValue.Compare(json, JValue.Parse(json.Serialize(2))));
            return json;
        }

        private static void AssertOrdering(params JValue[] values)
        {
            for (var i = 0; i < values.Length - 1; i++)
            {
                for (var k = i + 1; k < values.Length; k++)
                {
                    Assert.IsTrue(values[i] < values[k], $"'{values[i]} < {values[k]}' is false.");
                    Assert.IsTrue(values[k] > values[i], $"'{values[k]} > {values[i]}' is false.");
                }
            }
        }
    }
}
