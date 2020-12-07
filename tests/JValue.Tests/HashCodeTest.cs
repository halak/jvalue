using System;
using NUnit.Framework;

namespace Halak
{
    public class HashCodeTest
    {
        [Test]
        public void TestNull()
            => Assert.AreEqual(JValue.Null.GetHashCode(), JValue.Parse("null").GetHashCode());

        [Test]
        public void TestTrue()
        {
            Assert.AreEqual(JValue.True.GetHashCode(), JValue.Parse("true").GetHashCode());
            Assert.AreEqual(JValue.True.GetHashCode(), new JValue(true).GetHashCode());
        }

        [Test]
        public void TestFalse()
        {
            Assert.AreEqual(JValue.False.GetHashCode(), JValue.Parse("false").GetHashCode());
            Assert.AreEqual(JValue.False.GetHashCode(), new JValue(false).GetHashCode());
        }

        [Test]
        public void TestBoolean()
            => Assert.AreNotEqual(JValue.True.GetHashCode(), JValue.False.GetHashCode());

        [TestCase(123)]
        public void TestNumber(int n)
            => Assert.AreEqual(new JValue(n).GetHashCode(), new JValue((long)n).GetHashCode());

        [TestCase(123)]
        public void TestNumberAndDigitString(int n)
            => Assert.AreNotEqual(new JValue(n).GetHashCode(), JValue.Parse(FormattableString.Invariant($@"""{n}""")));

        [TestCase("마린")]
        [TestCase("hello world")]
        [TestCase("我思う故に我在り")]
        [TestCase("😃💁 People")]
        public void TestEscapedString(string s)
            => Assert.AreEqual(new JValue(s).GetHashCode(), JsonEncoding.EnsureAscii(s).GetHashCode());

        [TestCase(@"{""name"": ""harry"", ""age"": 30}")]
        [TestCase(@"[1, 2, 3, 4, 5, 6, 7]")]
        public void TestPaddedJson(string json)
        {
            var originalHashCode = JValue.Parse(json).GetHashCode();
            var minifiedJson = JValue.Parse(json).Serialize(0);
            var beautifiedJson = JValue.Parse(json).Serialize(2);
            Assert.AreNotEqual(json, minifiedJson);
            Assert.AreEqual(originalHashCode, JValue.Parse(minifiedJson).GetHashCode());
            Assert.AreEqual(originalHashCode, JValue.Parse(beautifiedJson).GetHashCode());
        }

        [Test]
        public void TestAnotherLocatedValue()
        {
            var array = JValue.Parse("[123, 123]");
            Assert.AreEqual(array[0].GetHashCode(), array[1].GetHashCode());
        }
    }
}
