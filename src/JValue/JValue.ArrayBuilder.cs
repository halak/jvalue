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
            private TextWriter writer;
            private bool used;

            public ArrayBuilder(int capacity)
                : this(new StringBuilder(capacity)) { }
            public ArrayBuilder(StringBuilder stringBuilder)
                : this(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)) { }
            public ArrayBuilder(TextWriter writer)
            {
                this.writer = writer;
                this.writer.Write('[');
                this.used = false;
            }

            public void Dispose()
            {
                Prepare(false);
                writer.Write(']');
            }

            public ArrayBuilder PushNull()
            {
                Prepare();
                writer.Write(JsonHelper.NullLiteral);
                return this;
            }

            public ArrayBuilder Push(bool value)
            {
                Prepare();
                writer.Write(value ? JsonHelper.TrueLiteral : JsonHelper.FalseLiteral);
                return this;
            }

            public ArrayBuilder Push(int value)
            {
                Prepare();
                JsonHelper.WriteInt32(writer, value);
                return this;
            }

            public ArrayBuilder Push(long value)
            {
                Prepare();
                JsonHelper.WriteInt64(writer, value);
                return this;
            }

            public ArrayBuilder Push(float value)
            {
                Prepare();
                writer.Write(value.ToString(CultureInfo.InvariantCulture));
                return this;
            }

            public ArrayBuilder Push(double value)
            {
                Prepare();
                writer.Write(value.ToString(CultureInfo.InvariantCulture));
                return this;
            }

            public ArrayBuilder Push(decimal value)
            {
                Prepare();
                writer.Write(value.ToString(CultureInfo.InvariantCulture));
                return this;
            }

            public ArrayBuilder Push(string value)
            {
                Prepare();
                JsonHelper.WriteEscapedString(writer, value);
                return this;
            }

            public ArrayBuilder Push(JValue value)
            {
                Prepare();
                value.WriteTo(writer);
                return this;
            }

            public ArrayBuilder PushArray(Action<ArrayBuilder> push)
            {
                Prepare();
                var subBuilder = new ArrayBuilder(writer);
                push(subBuilder);
                subBuilder.Dispose();
                return this;
            }

            public ArrayBuilder PushArray<T>(T value, Action<ArrayBuilder, T> push)
            {
                Prepare();
                var subBuilder = new ArrayBuilder(writer);
                push(subBuilder, value);
                subBuilder.Dispose();
                return this;
            }

            public ArrayBuilder PushObject(Action<ObjectBuilder> push)
            {
                Prepare();
                var subBuilder = new ObjectBuilder(writer);
                push(subBuilder);
                subBuilder.Dispose();
                return this;
            }

            public ArrayBuilder PushObject<T>(T value, Action<ObjectBuilder, T> push)
            {
                Prepare();
                var subBuilder = new ObjectBuilder(writer);
                push(subBuilder, value);
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
                    writer.Write('[');
                }

                if (used && hasNewElement)
                    writer.Write(',');
                else
                    used = true;
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
