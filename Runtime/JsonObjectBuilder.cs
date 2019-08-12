using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Halak
{
    public struct JsonObjectBuilder : IDisposable
    {
        private readonly JsonWriter writer;
        private readonly int startOffset;

        public JsonObjectBuilder(int capacity) : this(new JsonWriter(capacity)) { }
        public JsonObjectBuilder(StringBuilder stringBuilder) : this(new JsonWriter(stringBuilder)) { }
        public JsonObjectBuilder(TextWriter writer) : this(new JsonWriter(writer)) { }
        internal JsonObjectBuilder(JsonWriter writer)
        {
            this.writer = writer;
            this.writer.WriteStartObject();
            this.startOffset = writer.Offset;
        }

        public void Dispose()
        {
            writer.WriteEndObject();
        }

        public JsonObjectBuilder PutNull(string key)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.WriteNull();
            return this;
        }

        public JsonObjectBuilder Put(string key, bool value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder Put(string key, int value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder Put(string key, long value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder Put(string key, float value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder Put(string key, double value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder Put(string key, decimal value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder Put(string key, string value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder Put(string key, JValue value)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            writer.Write(value);
            return this;
        }

        public JsonObjectBuilder PutArray(string key, Action<JsonArrayBuilder> put)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            var arrayBuilder = new JsonArrayBuilder(writer);
            put(arrayBuilder);
            arrayBuilder.Dispose();
            return this;
        }

        public JsonObjectBuilder PutArray<T>(string key, T value, Action<JsonArrayBuilder, T> put)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            var arrayBuilder = new JsonArrayBuilder(writer);
            put(arrayBuilder, value);
            arrayBuilder.Dispose();
            return this;
        }

        public JsonObjectBuilder PutObject(string key, Action<JsonObjectBuilder> put)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            var objectBuilder = new JsonObjectBuilder(writer);
            put(objectBuilder);
            objectBuilder.Dispose();
            return this;
        }

        public JsonObjectBuilder PutObject<T>(string key, T value, Action<JsonObjectBuilder, T> put)
        {
            EnsureJsonWriter();
            writer.WriteCommaIf(startOffset);
            writer.WriteKey(key);
            var objectBuilder = new JsonObjectBuilder(writer);
            put(objectBuilder, value);
            objectBuilder.Dispose();
            return this;
        }

        public JValue Build()
        {
            Dispose();
            return writer.BuildJson();
        }

        private void EnsureJsonWriter()
        {
            if (writer == null)
                throw new InvalidOperationException("this object created by default constructor. please use parameterized constructor.");
        }

        #region Shorthand Methods
        public JsonObjectBuilder PutArray(string key, IEnumerable<string> values)
            => PutArrayOf(key, values, (arrayBuilder, value) => arrayBuilder.Push(value));

        public JsonObjectBuilder PutObject(string key, IEnumerable<KeyValuePair<string, string>> members)
            => PutObjectOf(key, members, (objectBuilder, member) => objectBuilder.Put(member.Key, member.Value));

        public JsonObjectBuilder PutArrayOfArray<T>(string key, IEnumerable<T> source, Action<JsonArrayBuilder, T> build)
            => PutArray(key, (source, build), Internal.ArrayOfArray);

        public JsonObjectBuilder PutObjectOfArray<T>(string key, IEnumerable<KeyValuePair<string, T>> source, Action<JsonArrayBuilder, T> build)
            => PutObject(key, (source, build), Internal.ObjectOfArray);

        public JsonObjectBuilder PutArrayOfObject<T>(string key, IEnumerable<T> source, Action<JsonObjectBuilder, T> build)
            => PutArray(key, (source, build), Internal.ArrayOfObject);

        public JsonObjectBuilder PutObjectOfObject<T>(string key, IEnumerable<KeyValuePair<string, T>> source, Action<JsonObjectBuilder, T> build)
            => PutObject(key, (source, build), Internal.ObjectOfObject);

        public JsonObjectBuilder PutArrayOf<T>(string key, IEnumerable<T> source, Func<JsonArrayBuilder, T, JsonArrayBuilder> build)
            => PutArray(key, (source, build), Internal.ArrayOf);

        public JsonObjectBuilder PutObjectOf<T>(string key, IEnumerable<KeyValuePair<string, T>> source, Func<JsonObjectBuilder, KeyValuePair<string, T>, JsonObjectBuilder> build)
            => PutObject(key, (source, build), Internal.ObjectOf);
        #endregion
    }
}
