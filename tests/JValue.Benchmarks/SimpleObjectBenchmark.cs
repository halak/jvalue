using BenchmarkDotNet.Attributes;

namespace Halak
{
    public class SimpleObjectBenchmark
    {
        private string json;

        [GlobalSetup]
        public void Setup()
        {
            json = $"{{\"x\": 123, \"y\": 456.789}}";
        }

        [Benchmark(Description = "JValue", Baseline = true)]
        public float HalakJValue()
        {
            return JValue.Parse(json)["y"];
        }

        [Benchmark(Description = "Json.NET")]
        public float NewtonsoftJson()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleObject>(json).y;
        }

        [Benchmark(Description = "Json.NET Linq")]
        public float NewtonsoftJsonLinq()
        {
            return Newtonsoft.Json.Linq.JObject.Parse(json).Value<float>("y");
        }

        private struct SimpleObject
        {
            public float x;
            public float y;

            public SimpleObject(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}
