using System;
using System.Collections.Generic;
using System.Text;

namespace Halak
{
    partial struct JValue
    {
        public struct ObjectBuilder
        {
            private StringBuilder builder;
            private int startIndex;

            public ObjectBuilder(int capacity)
            {
                this.builder = new StringBuilder(capacity);
                this.startIndex = 0;
            }

            public ObjectBuilder(StringBuilder builder)
            {
                this.builder = builder;
                this.startIndex = builder.Length;
            }

            public ObjectBuilder Put(string key, bool value)
            {
                Prepare();
                AppendKey(key);
                builder.Append(value);
                return this;
            }

            public ObjectBuilder Put(string key, int value)
            {
                Prepare();
                AppendKey(key);
                builder.Append(value);
                return this;
            }

            public ObjectBuilder Put(string key, long value)
            {
                Prepare();
                AppendKey(key);
                builder.Append(value);
                return this;
            }

            public ObjectBuilder Put(string key, float value)
            {
                Prepare();
                AppendKey(key);
                builder.Append(value);
                return this;
            }

            public ObjectBuilder Put(string key, string value)
            {
                Prepare();
                AppendKey(key);
                JsonHelper.EscapeTo(builder, value);
                return this;
            }

            public ObjectBuilder Put(string key, double value)
            {
                Prepare();
                AppendKey(key);
                builder.Append(value);
                return this;
            }

            public ObjectBuilder Put(string key, JValue value)
            {
                Prepare();
                AppendKey(key);
                if (value.Type != TypeCode.Null)
                    builder.Append(value.source, value.startIndex, value.length);
                else
                    builder.Append("null");
                return this;
            }

            public ObjectBuilder PutArray(string key, Action<ArrayBuilder> put)
            {
                Prepare();
                AppendKey(key);
                var subBuilder = new ArrayBuilder(builder);
                put(subBuilder);
                subBuilder.Close();
                return this;
            }

            public ObjectBuilder PutArray<T>(string key, T value, Action<ArrayBuilder, T> put)
            {
                Prepare();
                AppendKey(key);
                var subBuilder = new ArrayBuilder(builder);
                put(subBuilder, value);
                subBuilder.Close();
                return this;
            }

            public ObjectBuilder PutObject(string key, Action<ObjectBuilder> put)
            {
                Prepare();
                AppendKey(key);
                var subBuilder = new ObjectBuilder(builder);
                put(subBuilder);
                subBuilder.Close();
                return this;
            }

            public ObjectBuilder PutObject<T>(string key, T value, Action<ObjectBuilder, T> put)
            {
                Prepare();
                AppendKey(key);
                var subBuilder = new ObjectBuilder(builder);
                put(subBuilder, value);
                subBuilder.Close();
                return this;
            }

            public JValue Build()
            {
                Close();
                return new JValue(builder.ToString(), 0, builder.Length);
            }

            private void Prepare(bool hasNewElement = true)
            {
                if (builder == null)
                    builder = new StringBuilder(1024);
                if (builder.Length < startIndex)
                    throw new InvalidOperationException();

                if (builder.Length != startIndex)
                {
                    if (hasNewElement)
                        builder.Append(',');
                }
                else
                    builder.Append('{');
            }

            private void AppendKey(string key)
            {
                JsonHelper.EscapeTo(builder, key);
                builder.Append(':');
            }

            internal void Close()
            {
                Prepare(false);
                builder.Append('}');
            }
        }
    }
}
