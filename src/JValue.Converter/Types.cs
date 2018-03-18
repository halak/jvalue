using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Halak
{
    internal static class Types
    {
        private const BindingFlags PublicInstanceMethod = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;

        public static class Object
        {
            public static new MethodInfo ToString { get; }

            static Object()
            {
                var t = typeof(object);
                ToString = t.GetMethod(nameof(object.ToString), PublicInstanceMethod);
            }
        }

        public static class DateTime
        {
            public static new MethodInfo ToString { get; }

            static DateTime()
            {
                var t = typeof(System.DateTime);
                ToString = t.GetMethod(nameof(System.DateTime.ToString), new[] { typeof(string) });
            }
        }

        public static class ObjectBuilder
        {
            public static MethodInfo PutNull { get; }
            public static MethodInfo PutBoolean { get; }
            public static MethodInfo PutInt32 { get; }
            public static MethodInfo PutInt64 { get; }
            public static MethodInfo PutSingle { get; }
            public static MethodInfo PutDouble { get; }
            public static MethodInfo PutDecimal { get; }
            public static MethodInfo PutString { get; }

            static ObjectBuilder()
            {
                var t = typeof(JValue.ObjectBuilder);
                PutNull = t.GetMethod(nameof(JValue.ObjectBuilder.PutNull), PublicInstanceMethod);
                PutBoolean = t.GetMethod(nameof(JValue.ObjectBuilder.Put), new[] { typeof(string), typeof(bool) });
                PutInt32 = t.GetMethod(nameof(JValue.ObjectBuilder.Put), new[] { typeof(string), typeof(int) });
                PutInt64 = t.GetMethod(nameof(JValue.ObjectBuilder.Put), new[] { typeof(string), typeof(long) });
                PutSingle = t.GetMethod(nameof(JValue.ObjectBuilder.Put), new[] { typeof(string), typeof(float) });
                PutDouble = t.GetMethod(nameof(JValue.ObjectBuilder.Put), new[] { typeof(string), typeof(double) });
                PutDecimal = t.GetMethod(nameof(JValue.ObjectBuilder.Put), new[] { typeof(string), typeof(decimal) });
                PutString = t.GetMethod(nameof(JValue.ObjectBuilder.Put), new[] { typeof(string), typeof(string) });
            }
        }

        public static class ArrayBuilder
        {
            public static MethodInfo PushNull { get; }
            public static MethodInfo PushBoolean { get; }
            public static MethodInfo PushInt32 { get; }
            public static MethodInfo PushInt64 { get; }
            public static MethodInfo PushSingle { get; }
            public static MethodInfo PushDouble { get; }
            public static MethodInfo PushDecimal { get; }
            public static MethodInfo PushString { get; }

            static ArrayBuilder()
            {
                var t = typeof(JValue.ArrayBuilder);
                PushNull = t.GetMethod(nameof(JValue.ArrayBuilder.PushNull), PublicInstanceMethod);
                PushBoolean = t.GetMethod(nameof(JValue.ArrayBuilder.Push), new[] { typeof(string), typeof(bool) });
                PushInt32 = t.GetMethod(nameof(JValue.ArrayBuilder.Push), new[] { typeof(string), typeof(int) });
                PushInt64 = t.GetMethod(nameof(JValue.ArrayBuilder.Push), new[] { typeof(string), typeof(long) });
                PushSingle = t.GetMethod(nameof(JValue.ArrayBuilder.Push), new[] { typeof(string), typeof(float) });
                PushDouble = t.GetMethod(nameof(JValue.ArrayBuilder.Push), new[] { typeof(string), typeof(double) });
                PushDecimal = t.GetMethod(nameof(JValue.ArrayBuilder.Push), new[] { typeof(string), typeof(decimal) });
                PushString = t.GetMethod(nameof(JValue.ArrayBuilder.Push), new[] { typeof(string), typeof(string) });
            }
        }

        public static class IListObject
        {
            public static PropertyInfo Indexer { get; }

            static IListObject()
            {
                var t = typeof(IList<object>);

                Indexer = typeof(IList<object>).GetProperty("Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            }
        }

        static Types()
        {
        }
    }
}
