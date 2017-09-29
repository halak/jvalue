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

            Assert.AreEqual((bool)value, false);
            Assert.AreEqual((int)value, 0);
            Assert.AreEqual((long)value, 0L);
            Assert.AreEqual((float)value, 0.0f);
            Assert.AreEqual((double)value, 0.0);
            Assert.AreEqual((string)value, "");
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestFalseConversion()
        {
            var value = JValue.Parse("false");

            Assert.AreEqual((int)value, 0);
            Assert.AreEqual((long)value, 0L);
            Assert.AreEqual((float)value, 0.0f);
            Assert.AreEqual((double)value, 0.0);
            Assert.AreEqual((string)value, "false");
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestTrueConversion()
        {
            var value = JValue.Parse("true");

            Assert.AreEqual((int)value, 1);
            Assert.AreEqual((long)value, 1L);
            Assert.AreEqual((float)value, 1.0f);
            Assert.AreEqual((double)value, 1.0);
            Assert.AreEqual((string)value, "true");
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestZeroConversion()
        {
            var value = JValue.Parse("0");

            Assert.AreEqual((bool)value, false);
            Assert.AreEqual((string)value, "0");
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestOneConversion()
        {
            var value = JValue.Parse("1");

            Assert.AreEqual((bool)value, true);
            Assert.AreEqual((string)value, "1");
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestTwoConversion()
        {
            var value = JValue.Parse("2");

            Assert.AreEqual((bool)value, true);
            Assert.AreEqual((string)value, "2");
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestEmptyStringConversion()
        {
            var value = JValue.Parse("\"\"");

            Assert.AreEqual((bool)value, false);
            Assert.AreEqual((int)value, 0);
            Assert.AreEqual((long)value, 0L);
            Assert.AreEqual((float)value, 0.0f);
            Assert.AreEqual((double)value, 0.0);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestZeroStringConversion()
        {
            var value = JValue.Parse("\"0\"");

            Assert.AreEqual((bool)value, true);
            Assert.AreEqual((int)value, 0);
            Assert.AreEqual((long)value, 0L);
            Assert.AreEqual((float)value, 0.0f);
            Assert.AreEqual((double)value, 0.0);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestNumberStringConversion()
        {
            var value = JValue.Parse("\"123.456\"");

            Assert.AreEqual((bool)value, true);
            Assert.AreEqual((int)value, 0);
            Assert.AreEqual((long)value, 0L);
            Assert.AreEqual((float)value, 0.0f);
            Assert.AreEqual((double)value, 0.0);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }

        [TestMethod]
        public void TestStringConversion()
        {
            var value = JValue.Parse("\"hello world\"");

            Assert.AreEqual((bool)value, true);
            Assert.IsNotNull(value.AsArray());
            Assert.IsNotNull(value.AsObject());
            Assert.IsTrue(value.AsArray().SequenceEqual(new List<JValue>()));
            Assert.IsTrue(value.AsObject().SequenceEqual(new Dictionary<JValue, JValue>()));
        }
    }
}
