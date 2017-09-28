using System;
using System.Collections.Generic;
using System.Text;

namespace Halak
{
    partial struct JValue
    {
        public struct ArrayBuilder
        {
            private StringBuilder builder;
            private int startIndex;

            public ArrayBuilder(int capacity)
            {
                this.builder = new System.Text.StringBuilder(capacity);
                this.startIndex = 0;
            }

            public ArrayBuilder(StringBuilder builder)
            {
                this.builder = builder;
                this.startIndex = builder.Length;
            }

            public ArrayBuilder Push(bool value)
            {
                Prepare();
                builder.Append(value);
                return this;
            }

            public ArrayBuilder Push(int value)
            {
                Prepare();
                builder.Append(value);
                return this;
            }

            public ArrayBuilder Push(long value)
            {
                Prepare();
                builder.Append(value);
                return this;
            }

            public ArrayBuilder Push(float value)
            {
                Prepare();
                builder.Append(value);
                return this;
            }

            public ArrayBuilder Push(double value)
            {
                Prepare();
                builder.Append(value);
                return this;
            }

            public ArrayBuilder Push(string value)
            {
                Prepare();
                JsonHelper.EscapeTo(builder, value);
                return this;
            }

            public ArrayBuilder Push(JValue value)
            {
                Prepare();
                if (value.Type != TypeCode.Null)
                    builder.Append(value.source, value.startIndex, value.length);
                else
                    builder.Append("null");
                return this;
            }

            public ArrayBuilder PushArray(Action<ArrayBuilder> push)
            {
                Prepare();
                var subBuilder = new ArrayBuilder(builder);
                push(subBuilder);
                subBuilder.Close();
                return this;
            }

            public ArrayBuilder PushArray<T>(T value, Action<ArrayBuilder, T> push)
            {
                Prepare();
                var subBuilder = new ArrayBuilder(builder);
                push(subBuilder, value);
                subBuilder.Close();
                return this;
            }

            public ArrayBuilder PushObject(Action<ObjectBuilder> push)
            {
                Prepare();
                var subBuilder = new ObjectBuilder(builder);
                push(subBuilder);
                subBuilder.Close();
                return this;
            }

            public ArrayBuilder PushObject<T>(T value, Action<ObjectBuilder, T> push)
            {
                Prepare();
                var subBuilder = new ObjectBuilder(builder);
                push(subBuilder, value);
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
                    builder.Append('[');
            }

            internal void Close()
            {
                Prepare(false);
                builder.Append(']');
            }
        }
    }
}
