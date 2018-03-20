using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class ConversionTest
    {
        [TestMethod]
        public void TestNullConversion()
        {
            var value = JValue.Parse("null");

            Assert.AreEqual(false, (bool)value);
            Assert.AreEqual(0, (int)value);
            Assert.AreEqual(0L, (long)value, 0L);
            Assert.AreEqual(0.0f, (float)value);
            Assert.AreEqual(0.0, (double)value);
            Assert.AreEqual("", (string)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestFalseConversion()
        {
            var value = JValue.Parse("false");

            Assert.AreEqual(0, (int)value);
            Assert.AreEqual(0L, (long)value);
            Assert.AreEqual(0.0f, (float)value);
            Assert.AreEqual(0.0, (double)value);
            Assert.AreEqual("false", (string)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestTrueConversion()
        {
            var value = JValue.Parse("true");

            Assert.AreEqual(1, (int)value);
            Assert.AreEqual(1L, (long)value);
            Assert.AreEqual(1.0f, (float)value);
            Assert.AreEqual(1.0, (double)value);
            Assert.AreEqual("true", (string)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestZeroConversion()
        {
            var value = JValue.Parse("0");

            Assert.AreEqual(false, (bool)value);
            Assert.AreEqual("0", (string)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestOneConversion()
        {
            var value = JValue.Parse("1");

            Assert.AreEqual(true, (bool)value);
            Assert.AreEqual("1", (string)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestTwoConversion()
        {
            var value = JValue.Parse("2");

            Assert.AreEqual(true, (bool)value);
            Assert.AreEqual("2", (string)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestEmptyStringConversion()
        {
            var value = JValue.Parse("\"\"");

            Assert.AreEqual(false, (bool)value);
            Assert.AreEqual(0, (int)value);
            Assert.AreEqual(0L, (long)value);
            Assert.AreEqual(0.0f, (float)value);
            Assert.AreEqual(0.0, (double)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestZeroStringConversion()
        {
            var value = JValue.Parse("\"0\"");

            Assert.AreEqual(true, (bool)value);
            Assert.AreEqual(0, (int)value);
            Assert.AreEqual(0L, (long)value);
            Assert.AreEqual(0.0f, (float)value);
            Assert.AreEqual(0.0, (double)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestNumberStringConversion()
        {
            var value = JValue.Parse("\"123.456\"");

            Assert.AreEqual(true, (bool)value);
            Assert.AreEqual(123, (int)value);
            Assert.AreEqual(123L, (long)value);
            Assert.AreEqual(123.456f, (float)value);
            Assert.AreEqual(123.456, (double)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestStringConversion()
        {
            var value = JValue.Parse("\"hello world\"");

            Assert.AreEqual(true, (bool)value);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }
    }
}
