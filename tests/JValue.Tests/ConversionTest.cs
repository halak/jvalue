using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Halak
{
    public class ConversionTest
    {
        [Test]
        public void TestNullConversion()
        {
            var value = JValue.Parse("null");

            Assert.That((bool)value, Is.EqualTo(false));
            Assert.That((int)value, Is.EqualTo(0));
            Assert.That((long)value, Is.EqualTo(0L));
            Assert.That((float)value, Is.EqualTo(0.0f));
            Assert.That((double)value, Is.EqualTo(0.0));
            Assert.That((string)value, Is.EqualTo(""));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestFalseConversion()
        {
            var value = JValue.Parse("false");

            Assert.That((int)value, Is.EqualTo(0));
            Assert.That((long)value, Is.EqualTo(0L));
            Assert.That((float)value, Is.EqualTo(0.0f));
            Assert.That((double)value, Is.EqualTo(0.0));
            Assert.That((string)value, Is.EqualTo("false"));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestTrueConversion()
        {
            var value = JValue.Parse("true");

            Assert.That((int)value, Is.EqualTo(1));
            Assert.That((long)value, Is.EqualTo(1L));
            Assert.That((float)value, Is.EqualTo(1.0f));
            Assert.That((double)value, Is.EqualTo(1.0));
            Assert.That((string)value, Is.EqualTo("true"));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestZeroConversion()
        {
            var value = JValue.Parse("0");

            Assert.That((bool)value, Is.EqualTo(false));
            Assert.That((string)value, Is.EqualTo("0"));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestOneConversion()
        {
            var value = JValue.Parse("1");

            Assert.That((bool)value, Is.EqualTo(true));
            Assert.That((string)value, Is.EqualTo("1"));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestTwoConversion()
        {
            var value = JValue.Parse("2");

            Assert.That((bool)value, Is.EqualTo(true));
            Assert.That((string)value, Is.EqualTo("2"));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestEmptyStringConversion()
        {
            var value = JValue.Parse("\"\"");

            Assert.That((bool)value, Is.EqualTo(false));
            Assert.That((int)value, Is.EqualTo(0));
            Assert.That((long)value, Is.EqualTo(0L));
            Assert.That((float)value, Is.EqualTo(0.0f));
            Assert.That((double)value, Is.EqualTo(0.0));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestZeroStringConversion()
        {
            var value = JValue.Parse("\"0\"");

            Assert.That((bool)value, Is.EqualTo(true));
            Assert.That((int)value, Is.EqualTo(0));
            Assert.That((long)value, Is.EqualTo(0L));
            Assert.That((float)value, Is.EqualTo(0.0f));
            Assert.That((double)value, Is.EqualTo(0.0));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestNumberStringConversion()
        {
            var value = JValue.Parse("\"123.456\"");

            Assert.That((bool)value, Is.EqualTo(true));
            Assert.That((int)value, Is.EqualTo(123));
            Assert.That((long)value, Is.EqualTo(123L));
            Assert.That((float)value, Is.EqualTo(123.456f));
            Assert.That((double)value, Is.EqualTo(123.456));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }

        [Test]
        public void TestStringConversion()
        {
            var value = JValue.Parse("\"hello world\"");

            Assert.That((bool)value, Is.EqualTo(true));
            Assert.That(value.ToArray(), Is.Not.Null);
            Assert.That(value.ToObject(), Is.Not.Null);
            Assert.That(value.ToArray().SequenceEqual(new List<JValue>()), Is.True);
            Assert.That(value.ToObject().SequenceEqual(new Dictionary<JValue, JValue>()), Is.True);
        }
    }
}