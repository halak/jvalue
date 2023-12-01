using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Halak
{
    public struct JsonArrayBuilder : IDisposable
    {
        private readonly JsonWriter writer;
        private readonly int startOffset;

        public JsonArrayBuilder(int capacity) : this(new JsonWriter(capacity)) { }
        public JsonArrayBuilder(StringBuilder stringBuilder) : this(new JsonWriter(stringBuilder)) { }
        public JsonArrayBuilder(TextWriter writer) : this(new JsonWriter(writer)) { }
        internal JsonArrayBuilder(JsonWriter writer)
        {
            this.writer = writer;
            this.writer.WriteStartArray();
            this.startOffset = writer.Offset;
        }

        public void Dispose()
        {
            writer.WriteEndArray();
        }

        public JsonArrayBuilder PushNull()
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.WriteNull();
            return this;
        }

        public JsonArrayBuilder Push(bool value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(int value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(uint value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(long value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(ulong value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(float value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(double value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(decimal value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(string value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder Push(JValue value)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            writer.Write(value);
            return this;
        }

        public JsonArrayBuilder PushArray(Action<JsonArrayBuilder> push)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            var arrayBuilder = new JsonArrayBuilder(writer);
            push(arrayBuilder);
            arrayBuilder.Dispose();
            return this;
        }

        public JsonArrayBuilder PushArray<T>(T value, Action<JsonArrayBuilder, T> push)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            var arrayBuilder = new JsonArrayBuilder(writer);
            push(arrayBuilder, value);
            arrayBuilder.Dispose();
            return this;
        }

        public JsonArrayBuilder PushObject(Action<JsonObjectBuilder> push)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            var objectBuilder = new JsonObjectBuilder(writer);
            push(objectBuilder);
            objectBuilder.Dispose();
            return this;
        }

        public JsonArrayBuilder PushObject<T>(T value, Action<JsonObjectBuilder, T> push)
        {
            EnsureState();
            writer.WriteCommaIf(startOffset);
            var objectBuilder = new JsonObjectBuilder(writer);
            push(objectBuilder, value);
            objectBuilder.Dispose();
            return this;
        }

        public JValue Build()
        {
            Dispose();
            return writer.BuildJson();
        }

        #region Shorthand Methods
        public JsonArrayBuilder PushArray(IEnumerable<bool> elements)
            => PushArrayOf(elements, (arrayBuilder, value) => arrayBuilder.Push(value));

        public JsonArrayBuilder PushArray(IEnumerable<int> elements)
            => PushArrayOf(elements, (arrayBuilder, value) => arrayBuilder.Push(value));

        public JsonArrayBuilder PushArray(IEnumerable<string> elements)
            => PushArrayOf(elements, (arrayBuilder, value) => arrayBuilder.Push(value));

        public JsonArrayBuilder PushObject(IEnumerable<KeyValuePair<string, string>> members)
            => PushObjectOf(members, (objectBuilder, member) => objectBuilder.Put(member.Key, member.Value));

        public JsonArrayBuilder PushObject(IEnumerable<KeyValuePair<string, JValue>> members)
            => PushObjectOf(members, (objectBuilder, member) => objectBuilder.Put(member.Key, member.Value));

        public JsonArrayBuilder PushArrayOfArray<T>(IEnumerable<T> source, Action<JsonArrayBuilder, T> build)
            => PushArray((source, build), Internal.ArrayOfArray);

        public JsonArrayBuilder PushObjectOfArray<T>(IEnumerable<KeyValuePair<string, T>> source, Action<JsonArrayBuilder, T> build)
            => PushObject((source, build), Internal.ObjectOfArray);

        public JsonArrayBuilder PushArrayOfObject<T>(IEnumerable<T> source, Action<JsonObjectBuilder, T> build)
            => PushArray((source, build), Internal.ArrayOfObject);

        public JsonArrayBuilder PushObjectOfObject<T>(IEnumerable<KeyValuePair<string, T>> source, Action<JsonObjectBuilder, T> build)
            => PushObject((source, build), Internal.ObjectOfObject);

        public JsonArrayBuilder PushArrayOf<T>(IEnumerable<T> source, Func<JsonArrayBuilder, T, JsonArrayBuilder> build)
            => PushArray((source, build), Internal.ArrayOf);

        public JsonArrayBuilder PushObjectOf<T>(IEnumerable<KeyValuePair<string, T>> source, Func<JsonObjectBuilder, KeyValuePair<string, T>, JsonObjectBuilder> build)
            => PushObject((source, build), Internal.ObjectOf);

        public JsonArrayBuilder PushNullIf(bool condition) => condition ? PushNull() : this;
        public JsonArrayBuilder PushIf(bool condition, bool value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, int value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, uint value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, long value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, ulong value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, float value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, double value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, decimal value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, string value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushIf(bool condition, JValue value) => condition ? Push(value) : this;
        public JsonArrayBuilder PushArrayIf(bool condition, Action<JsonArrayBuilder> push) => condition ? PushArray(push) : this;
        public JsonArrayBuilder PushArrayIf<T>(bool condition, T value, Action<JsonArrayBuilder, T> push) => condition ? PushArray(value, push) : this;
        public JsonArrayBuilder PushObjectIf(bool condition, Action<JsonObjectBuilder> push) => condition ? PushObject(push) : this;
        public JsonArrayBuilder PushObjectIf<T>(bool condition, T value, Action<JsonObjectBuilder, T> push) => condition ? PushObject(value, push) : this;

        public JsonArrayBuilder PushArrayIf(bool condition, IEnumerable<int> elements) => condition ? PushArray(elements) : this;
        public JsonArrayBuilder PushArrayIf(bool condition, IEnumerable<string> elements) => condition ? PushArray(elements) : this;
        public JsonArrayBuilder PushObjectIf(bool condition, IEnumerable<KeyValuePair<string, string>> members) => condition ? PushObject(members) : this;
        public JsonArrayBuilder PushArrayOfArrayIf<T>(bool condition, IEnumerable<T> source, Action<JsonArrayBuilder, T> build) => condition ? PushArrayOfArray(source, build) : this;
        public JsonArrayBuilder PushObjectOfArrayIf<T>(bool condition, IEnumerable<KeyValuePair<string, T>> source, Action<JsonArrayBuilder, T> build) => condition ? PushObjectOfArray(source, build) : this;
        public JsonArrayBuilder PushArrayOfObjectIf<T>(bool condition, IEnumerable<T> source, Action<JsonObjectBuilder, T> build) => condition ? PushArrayOfObject(source, build) : this;
        public JsonArrayBuilder PushObjectOfObjectIf<T>(bool condition, IEnumerable<KeyValuePair<string, T>> source, Action<JsonObjectBuilder, T> build) => condition ? PushObjectOfObject(source, build) : this;
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureState()
        {
            if (writer == null)
                throw new InvalidOperationException("this object created by default constructor. please use parameterized constructor.");
        }
    }
}
