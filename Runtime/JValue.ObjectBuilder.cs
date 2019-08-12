using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;

namespace Halak
{
    partial struct JValue
    {
        public struct ObjectBuilder : IDisposable
        {
            private readonly JsonWriter writer;
            private readonly int startOffset;

            public ObjectBuilder(int capacity) : this(new JsonWriter(capacity)) { }
            public ObjectBuilder(StringBuilder stringBuilder) : this(new JsonWriter(stringBuilder)) { }
            public ObjectBuilder(TextWriter writer) : this(new JsonWriter(writer)) { }
            internal ObjectBuilder(JsonWriter writer)
            {
                this.writer = writer;
                this.writer.WriteStartObject();
                this.startOffset = writer.Offset;
            }

            public void Dispose()
            {
                writer.WriteEndObject();
            }

            public ObjectBuilder PutNull(string key)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.WriteNull();
                return this;
            }

            public ObjectBuilder Put(string key, bool value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder Put(string key, int value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder Put(string key, long value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder Put(string key, float value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder Put(string key, double value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder Put(string key, decimal value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder Put(string key, string value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder Put(string key, JValue value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                writer.Write(value);
                return this;
            }

            public ObjectBuilder PutArray(string key, Action<ArrayBuilder> put)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                var arrayBuilder = new ArrayBuilder(writer);
                put(arrayBuilder);
                arrayBuilder.Dispose();
                return this;
            }

            public ObjectBuilder PutArray<T>(string key, T value, Action<ArrayBuilder, T> put)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                var arrayBuilder = new ArrayBuilder(writer);
                put(arrayBuilder, value);
                arrayBuilder.Dispose();
                return this;
            }

            public ObjectBuilder PutObject(string key, Action<ObjectBuilder> put)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                var objectBuilder = new ObjectBuilder(writer);
                put(objectBuilder);
                objectBuilder.Dispose();
                return this;
            }

            public ObjectBuilder PutObject<T>(string key, T value, Action<ObjectBuilder, T> put)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteKey(key);
                var objectBuilder = new ObjectBuilder(writer);
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
            public ObjectBuilder PutArray(string key, IEnumerable<string> values)
                => PutArrayOf(key, values, (arrayBuilder, value) => arrayBuilder.Push(value));

            public ObjectBuilder PutObject(string key, IEnumerable<KeyValuePair<string, string>> members)
                => PutObjectOf(key, members, (objectBuilder, member) => objectBuilder.Put(member.Key, member.Value));

            public ObjectBuilder PutArrayOfArray<T>(string key, IEnumerable<T> source, Action<ArrayBuilder, T> build)
                => PutArray(key, (source, build), Internal.ArrayOfArray);

            public ObjectBuilder PutObjectOfArray<T>(string key, IEnumerable<KeyValuePair<string, T>> source, Action<ArrayBuilder, T> build)
                => PutObject(key, (source, build), Internal.ObjectOfArray);

            public ObjectBuilder PutArrayOfObject<T>(string key, IEnumerable<T> source, Action<ObjectBuilder, T> build)
                => PutArray(key, (source, build), Internal.ArrayOfObject);

            public ObjectBuilder PutObjectOfObject<T>(string key, IEnumerable<KeyValuePair<string, T>> source, Action<ObjectBuilder, T> build)
                => PutObject(key, (source, build), Internal.ObjectOfObject);

            public ObjectBuilder PutArrayOf<T>(string key, IEnumerable<T> source, Func<ArrayBuilder, T, ArrayBuilder> build)
                => PutArray(key, (source, build), Internal.ArrayOf);

            public ObjectBuilder PutObjectOf<T>(string key, IEnumerable<KeyValuePair<string, T>> source, Func<ObjectBuilder, KeyValuePair<string, T>, ObjectBuilder> build)
                => PutObject(key, (source, build), Internal.ObjectOf);
            #endregion
        }
    }
}
