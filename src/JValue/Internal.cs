using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Halak
{
    internal static class Internal
    {
        public static void ArrayOfArray<T>(JValue.ArrayBuilder builder, (IEnumerable<T> source, Action<JValue.ArrayBuilder, T> build) state)
        {
            foreach (var item in state.source.ToEfficient())
                builder.PushArray(item, state.build);
        }

        public static void ObjectOfArray<T>(JValue.ObjectBuilder builder, (IEnumerable<KeyValuePair<string, T>> source, Action<JValue.ArrayBuilder, T> build) state)
        {
            foreach (var item in state.source.ToEfficient())
                builder.PutArray(item.Key, item.Value, state.build);
        }

        public static void ArrayOfObject<T>(JValue.ArrayBuilder builder, (IEnumerable<T> source, Action<JValue.ObjectBuilder, T> build) state)
        {
            foreach (var item in state.source.ToEfficient())
                builder.PushObject(item, state.build);
        }

        public static void ObjectOfObject<T>(JValue.ObjectBuilder builder, (IEnumerable<KeyValuePair<string, T>> source, Action<JValue.ObjectBuilder, T> build) state)
        {
            foreach (var item in state.source.ToEfficient())
                builder.PutObject(item.Key, item.Value, state.build);
        }

        public static void ArrayOf<T>(JValue.ArrayBuilder builder, (IEnumerable<T> source, Func<JValue.ArrayBuilder, T, JValue.ArrayBuilder> build) state)
        {
            foreach (var item in state.source.ToEfficient())
                builder = state.build(builder, item);
        }

        public static void ObjectOf<T>(JValue.ObjectBuilder builder, (IEnumerable<KeyValuePair<string, T>> source, Func<JValue.ObjectBuilder, KeyValuePair<string, T>, JValue.ObjectBuilder> build) state)
        {
            foreach (var item in state.source.ToEfficient())
                builder = state.build(builder, item);
        }

        internal static OptimizedEnumerator<T> ToEfficient<T>(this IEnumerable<T> source) => OptimizedEnumerator<T>.From(source);
        internal struct OptimizedEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> enumerator;
            private readonly IReadOnlyList<T> list;
            private readonly T[] array;
            private int index;

            public T Current
            {
                get
                {
                    if (array != null)
                        return array[index];
                    else if (list != null)
                        return list[index];
                    else
                        return enumerator.Current;
                }
            }

            object IEnumerator.Current => Current;

            public OptimizedEnumerator(IEnumerator<T> enumerator, IReadOnlyList<T> list, T[] array)
            {
                this.enumerator = enumerator;
                this.list = list;
                this.array = array;
                this.index = -1;
            }

            public bool MoveNext()
            {
                index++;

                if (array != null)
                    return index < array.Length;
                else if (list != null)
                    return index < list.Count;
                else
                    return enumerator.MoveNext();
            }

            public void Reset()
            {
                index = -1;
                enumerator?.Reset();
            }

            public void Dispose()
            {
                enumerator?.Dispose();
            }

            public OptimizedEnumerator<T> GetEnumerator() => this; // used for foreach

            public static OptimizedEnumerator<T> From(IEnumerable<T> source)
            {
                if (source is T[] array)
                    return new OptimizedEnumerator<T>(null, array, array);
                else if (source is IReadOnlyList<T> list)
                    return new OptimizedEnumerator<T>(null, list, null);
                else
                    return new OptimizedEnumerator<T>(source.GetEnumerator(), null, null);
            }
        }
    }
}
