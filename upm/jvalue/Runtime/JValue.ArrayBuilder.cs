using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;

namespace Halak
{
    partial struct JValue
    {
        public struct ArrayBuilder : IDisposable
        {
            private readonly JsonWriter writer;
            private readonly int startOffset;

            public ArrayBuilder(int capacity) : this(new JsonWriter(capacity)) { }
            public ArrayBuilder(StringBuilder stringBuilder) : this(new JsonWriter(stringBuilder)) { }
            public ArrayBuilder(TextWriter writer) : this(new JsonWriter(writer)) { }
            internal ArrayBuilder(JsonWriter writer)
            {
                this.writer = writer;
                this.writer.WriteStartArray();
                this.startOffset = writer.Offset;
            }

            public void Dispose()
            {
                writer.WriteEndArray();
            }

            public ArrayBuilder PushNull()
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.WriteNull();
                return this;
            }

            public ArrayBuilder Push(bool value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder Push(int value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder Push(long value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder Push(float value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder Push(double value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder Push(decimal value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder Push(string value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder Push(JValue value)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                writer.Write(value);
                return this;
            }

            public ArrayBuilder PushArray(Action<ArrayBuilder> push)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                var subBuilder = new ArrayBuilder(writer);
                push(subBuilder);
                subBuilder.Dispose();
                return this;
            }

            public ArrayBuilder PushArray<T>(T value, Action<ArrayBuilder, T> push)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                var arrayBuilder = new ArrayBuilder(writer);
                push(arrayBuilder, value);
                arrayBuilder.Dispose();
                return this;
            }

            public ArrayBuilder PushObject(Action<ObjectBuilder> push)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                var objectBuilder = new ObjectBuilder(writer);
                push(objectBuilder);
                objectBuilder.Dispose();
                return this;
            }

            public ArrayBuilder PushObject<T>(T value, Action<ObjectBuilder, T> push)
            {
                EnsureJsonWriter();
                writer.WriteCommaIf(startOffset);
                var objectBuilder = new ObjectBuilder(writer);
                push(objectBuilder, value);
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
            public ArrayBuilder PushArray(IEnumerable<string> values)
                => PushArrayOf(values, (arrayBuilder, value) => arrayBuilder.Push(value));

            public ArrayBuilder PushObject(IEnumerable<KeyValuePair<string, string>> members)
                => PushObjectOf(members, (objectBuilder, member) => objectBuilder.Put(member.Key, member.Value));

            public ArrayBuilder PushArrayOfArray<T>(IEnumerable<T> source, Action<ArrayBuilder, T> build)
                => PushArray((source, build), Internal.ArrayOfArray);

            public ArrayBuilder PushObjectOfArray<T>(IEnumerable<KeyValuePair<string, T>> source, Action<ArrayBuilder, T> build)
                => PushObject((source, build), Internal.ObjectOfArray);

            public ArrayBuilder PushArrayOfObject<T>(IEnumerable<T> source, Action<ObjectBuilder, T> build)
                => PushArray((source, build), Internal.ArrayOfObject);

            public ArrayBuilder PushObjectOfObject<T>(IEnumerable<KeyValuePair<string, T>> source, Action<ObjectBuilder, T> build)
                => PushObject((source, build), Internal.ObjectOfObject);

            public ArrayBuilder PushArrayOf<T>(IEnumerable<T> source, Func<ArrayBuilder, T, ArrayBuilder> build)
                => PushArray((source, build), Internal.ArrayOf);

            public ArrayBuilder PushObjectOf<T>(IEnumerable<KeyValuePair<string, T>> source, Func<ObjectBuilder, KeyValuePair<string, T>, ObjectBuilder> build)
                => PushObject((source, build), Internal.ObjectOf);
            #endregion
        }
    }
}
