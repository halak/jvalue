using System;
using NUnit.Framework;

namespace Halak
{
    public class HashCodeTest
    {
        [Test]
        public void TestNull()
            => Assert.That(JValue.Parse("null").GetHashCode(), Is.EqualTo(JValue.Null.GetHashCode()));

        [Test]
        public void TestTrue()
        {
            Assert.That(JValue.Parse("true").GetHashCode(), Is.EqualTo(JValue.True.GetHashCode()));
            Assert.That(new JValue(true).GetHashCode(), Is.EqualTo(JValue.True.GetHashCode()));
        }

        [Test]
        public void TestFalse()
        {
            Assert.That(JValue.Parse("false").GetHashCode(), Is.EqualTo(JValue.False.GetHashCode()));
            Assert.That(new JValue(false).GetHashCode(), Is.EqualTo(JValue.False.GetHashCode()));
        }

        [Test]
        public void TestBoolean()
            => Assert.That(JValue.False.GetHashCode(), Is.Not.EqualTo(JValue.True.GetHashCode()));

        [TestCase(123)]
        public void TestNumber(int n)
            => Assert.That(new JValue((long)n).GetHashCode(), Is.EqualTo(new JValue(n).GetHashCode()));

        [TestCase(123)]
        public void TestNumberAndDigitString(int n)
            => Assert.That(JValue.Parse(FormattableString.Invariant($@"""{n}""")).GetHashCode(), Is.Not.EqualTo(new JValue(n).GetHashCode()));

        [TestCase("마린")]
        [TestCase("hello world")]
        [TestCase("我思う故に我在り")]
        [TestCase("😃💁 People")]
        public void TestEscapedString(string s)
            => Assert.That(JsonEncoding.EnsureAscii(s).GetHashCode(), Is.EqualTo(new JValue(s).GetHashCode()));

        [TestCase(@"{""name"": ""harry"", ""age"": 30}")]
        [TestCase(@"[1, 2, 3, 4, 5, 6, 7]")]
        public void TestPaddedJson(string json)
        {
            var originalHashCode = JValue.Parse(json).GetHashCode();
            var minifiedJson = JValue.Parse(json).Serialize(0);
            var prettyJson = JValue.Parse(json).Serialize(2);
            Assert.That(minifiedJson, Is.Not.EqualTo(json));
            Assert.That(JValue.Parse(minifiedJson).GetHashCode(), Is.EqualTo(originalHashCode));
            Assert.That(JValue.Parse(prettyJson).GetHashCode(), Is.EqualTo(originalHashCode));
        }

        [Test]
        public void TestAnotherLocatedValue()
        {
            var array = JValue.Parse("[123, 123]");
            Assert.That(array[1].GetHashCode(), Is.EqualTo(array[0].GetHashCode()));
        }
    }
}