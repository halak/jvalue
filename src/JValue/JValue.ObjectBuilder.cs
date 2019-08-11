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
            private TextWriter writer;
            private bool used;

            public ObjectBuilder(int capacity)
                : this(new StringBuilder(capacity)) { }
            public ObjectBuilder(StringBuilder stringBuilder)
                : this(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)) { }
            public ObjectBuilder(TextWriter writer)
            {
                this.writer = writer;
                this.writer.Write('{');
                this.used = false;
            }

            public void Dispose()
            {
                Prepare(false);
                writer.Write('}');
            }

            public ObjectBuilder PutNull(string key)
            {
                Prepare();
                WriteKey(key);
                writer.Write(JsonHelper.NullLiteral);
                return this;
            }

            public ObjectBuilder Put(string key, bool value)
            {
                Prepare();
                WriteKey(key);
                writer.Write(value ? JsonHelper.TrueLiteral : JsonHelper.FalseLiteral);
                return this;
            }

            public ObjectBuilder Put(string key, int value)
            {
                Prepare();
                WriteKey(key);
                JsonHelper.WriteInt32(writer, value);
                return this;
            }

            public ObjectBuilder Put(string key, long value)
            {
                Prepare();
                WriteKey(key);
                JsonHelper.WriteInt64(writer, value);
                return this;
            }

            public ObjectBuilder Put(string key, float value)
            {
                Prepare();
                WriteKey(key);
                writer.Write(value.ToString(CultureInfo.InvariantCulture));
                return this;
            }

            public ObjectBuilder Put(string key, double value)
            {
                Prepare();
                WriteKey(key);
                writer.Write(value.ToString(CultureInfo.InvariantCulture));
                return this;
            }

            public ObjectBuilder Put(string key, decimal value)
            {
                Prepare();
                WriteKey(key);
                writer.Write(value.ToString(CultureInfo.InvariantCulture));
                return this;
            }

            public ObjectBuilder Put(string key, string value)
            {
                Prepare();
                WriteKey(key);
                JsonHelper.WriteEscapedString(writer, value);
                return this;
            }

            public ObjectBuilder Put(string key, JValue value)
            {
                Prepare();
                WriteKey(key);
                value.WriteTo(writer);
                return this;
            }

            public ObjectBuilder PutArray(string key, Action<ArrayBuilder> put)
            {
                Prepare();
                WriteKey(key);
                var subBuilder = new ArrayBuilder(writer);
                put(subBuilder);
                subBuilder.Dispose();
                return this;
            }

            public ObjectBuilder PutArray<T>(string key, T value, Action<ArrayBuilder, T> put)
            {
                Prepare();
                WriteKey(key);
                var subBuilder = new ArrayBuilder(writer);
                put(subBuilder, value);
                subBuilder.Dispose();
                return this;
            }

            public ObjectBuilder PutObject(string key, Action<ObjectBuilder> put)
            {
                Prepare();
                WriteKey(key);
                var subBuilder = new ObjectBuilder(writer);
                put(subBuilder);
                subBuilder.Dispose();
                return this;
            }

            public ObjectBuilder PutObject<T>(string key, T value, Action<ObjectBuilder, T> put)
            {
                Prepare();
                WriteKey(key);
                var subBuilder = new ObjectBuilder(writer);
                put(subBuilder, value);
                subBuilder.Dispose();
                return this;
            }

            public JValue Build()
            {
                Dispose();
                return writer.BuildJson();
            }

            private void Prepare(bool hasNewElement = true)
            {
                if (writer == null)
                {
                    writer = new StringWriter(new StringBuilder(1024), CultureInfo.InvariantCulture);
                    writer.Write('{');
                }

                if (used && hasNewElement)
                    writer.Write(',');
                else
                    used = true;
            }

            private void WriteKey(string key)
            {
                JsonHelper.WriteEscapedString(writer, key);
                writer.Write(':');
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
