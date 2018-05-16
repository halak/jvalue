using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halak
{
    [TestClass]
    public class JsonConverterTest
    {
        private JsonConverter defaultConverter;

        [TestInitialize]
        public void TestInitialize()
        {
            defaultConverter = new JsonConverter();
        }

        [TestMethod]
        public void TestSimpleAnonymousObjectSerialization()
        {
            Assert.AreEqual(@"{""X"":10,""Y"":20}",
                ToJsonString(new { X = 10.0f, Y = 20.0f }));
            Assert.AreEqual(@"{""Name"":""Halak"",""Birthday"":""1988-06-12T00:00:00.0000000"",""Money"":100.0}",
                ToJsonString(new { Name = "Halak", Birthday = new DateTime(1988, 06, 12), Money = 100.0m }));
        }

        [TestMethod]
        public void TestCompositeObjectSerialization()
        {
            Assert.AreEqual(@"{""X"":10,""Y"":20,""Z"":30}",
                ToJsonString(Composite(new { X = 10.0f, Y = 20.0f }, new { Z = 30.0f })));
            Assert.AreEqual(@"{""X"":10,""Y"":30,""Z"":30}",
                ToJsonString(Composite(new { X = 10.0f, Y = 20.0f }, new { Y = 30.0f, Z = 30.0f })));
        }

        private string ToJsonString(object obj, JsonConverter converter = null)
            => (converter ?? defaultConverter).FromObject(obj).ToString();

        private static object Composite(params object[] objs)
            => new CompositeObject(objs);
    }
}
