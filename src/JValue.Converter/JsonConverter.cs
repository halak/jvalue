using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Halak
{
    public sealed partial class JsonConverter
    {
        private readonly ConcurrentBag<StringBuilder> stringBuilderPool;
        private readonly ConcurrentDictionary<Type, TypeContract> contractTable;
        private readonly Func<Type, TypeContract> contractFactory;
        private readonly ConcurrentDictionary<Type, JsonMember[]> jsonMembersCache;
        private readonly Func<Type, JsonMember[]> jsonMembersFactory;
        private readonly MemberTypes targetMembers;
        private readonly ConstantExpression dateTimeFormat;

        public string DateTimeFormat => (string)dateTimeFormat.Value;

        public JsonConverter(MemberTypes targetMembers = MemberTypes.Property, string dateTimeFormat = "O")
        {
            targetMembers &= (MemberTypes.Property | MemberTypes.Field);

            this.stringBuilderPool = new ConcurrentBag<StringBuilder>();
            this.contractTable = new ConcurrentDictionary<Type, TypeContract>(TypeComparer.Singleton);
            this.jsonMembersCache = new ConcurrentDictionary<Type, JsonMember[]>(TypeComparer.Singleton);
            this.contractFactory = Create;
            this.jsonMembersFactory = GetJsonMembersInternal;
            this.targetMembers = targetMembers;
            this.dateTimeFormat = Expression.Constant(dateTimeFormat);
        }

        public JValue FromObject(object obj) => (obj != null) ? SerializeInternal(GetContract(obj.GetType()), obj) : JValue.Null;
        public JValue FromObject<T>(T obj) => IsNotNull(obj) ? SerializeInternal(GetContract<T>(obj.GetType()), obj) : JValue.Null;
        public JValue FromObject<T>(T? obj) where T : struct => obj.HasValue ? SerializeInternal(GetContract<T>(), obj.Value) : JValue.Null;

        private TypeContract GetContract(Type type) => contractTable.GetOrAdd(type, contractFactory);
        private TypeContract<T> GetContract<T>(Type type = null) => (TypeContract<T>)GetContract(type ?? typeof(T));

        private JValue SerializeInternal(TypeContract contract, object obj)
        {
            var stringBuilder = RentStringBuilder();
            try
            {
                return contract.Build(stringBuilder, obj);
            }
            finally
            {
                ReturnStringBuilder(stringBuilder);
            }
        }

        private JValue SerializeInternal<T>(TypeContract<T> contract, T obj)
        {
            var stringBuilder = RentStringBuilder();
            try
            {
                return contract.Build(stringBuilder, obj);
            }
            finally
            {
                ReturnStringBuilder(stringBuilder);
            }
        }

        private StringBuilder RentStringBuilder() => stringBuilderPool.TryTake(out var stringBuilder) ? stringBuilder : new StringBuilder();
        private void ReturnStringBuilder(StringBuilder stringBuilder)
        {
            stringBuilder.Clear();
            stringBuilderPool.Add(stringBuilder);
        }

        private TypeContract Create(Type type)
        {
            var jsonMembers = GetJsonMembers(type);

            var parameters = EmitBuildObjectParameters(type);
            var statements = new List<Expression>(jsonMembers.Length * 2);

            foreach (var member in jsonMembers)
                statements.Add(EmitPutStatement(parameters.Builder, parameters.Source, member));

            var typedBuildObjectType = typeof(BuildObject<>).MakeGenericType(type);

            var compiledMethod = Expression.Lambda(
                typedBuildObjectType,
                Expression.Block(statements),
                parameters.Builder,
                parameters.Source).Compile();

            var typedContractType = typeof(TypeContract<>).MakeGenericType(type);
            var typedContractConstructor = typedContractType.GetConstructor(new Type[] { typedBuildObjectType });
            return (TypeContract)typedContractConstructor.Invoke(new object[] { compiledMethod });
        }

        private JsonMember[] GetJsonMembers(Type type) => jsonMembersCache.GetOrAdd(type, jsonMembersFactory);
        private JsonMember[] GetJsonMembersInternal(Type type)
        {
            const BindingFlags DeclaredInstanceMembers =
                BindingFlags.Instance | BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic;

            MemberInfo[] members = null;
            if (targetMembers == MemberTypes.Property)
                members = type.GetProperties(DeclaredInstanceMembers);
            else if (targetMembers == MemberTypes.Field)
                members = type.GetFields(DeclaredInstanceMembers);
            else
                members = type.GetMembers(DeclaredInstanceMembers);

            var list = new List<JsonMember>(members.Length);
            foreach (var member in members)
            {
                var jsonMember = TryCreateJsonMember(member);
                if (jsonMember != null)
                    list.Add(jsonMember);
            }

            return list.ToArray();
        }

        private JsonMember TryCreateJsonMember(MemberInfo member)
        {
            if ((member.MemberType & targetMembers) == 0)
                return null;

            if (member.MemberType == MemberTypes.Property && member is PropertyInfo property)
            {
                var getMethod = property.GetMethod;
                if (getMethod.IsStatic)
                    return null;

                return new JsonMember(GetMemberName(), property);
            }
            else if (member.MemberType == MemberTypes.Field && member is FieldInfo field)
            {
                if (field.IsStatic)
                    return null;
                if (field.Name.IndexOf('<') != -1)  // 자동 프로퍼티를 위하여 생성된 익명 필드
                    return null;
                if (member.GetCustomAttribute<NonSerializedAttribute>() != null)
                    return null;

                return new JsonMember(GetMemberName(), field);
            }
            else
                return null;

            string GetMemberName() => member.Name;
        }

        private (ParameterExpression Builder, ParameterExpression Source) EmitBuildObjectParameters(Type objType)
            => (Expression.Parameter(typeof(JValue.ObjectBuilder), "builder"), Expression.Parameter(objType, "obj"));
        private (ParameterExpression Builder, ParameterExpression Source) EmitBuildArrayParameters(Type objType)
            => (Expression.Parameter(typeof(JValue.ArrayBuilder), "builder"), Expression.Parameter(objType, "obj"));

        private Expression EmitPutStatement(Expression builder, Expression instance, JsonMember member)
            => EmitPutStatement(builder, member.NameExpression, member.EmitAccessExpression(instance), member.ValueType, member.ValueTypeCode);
        private Expression EmitPutStatement(Expression builder, Expression name, Expression value, Type valueType, TypeCode valueTypeCode)
        {
            switch (valueTypeCode)
            {
                case TypeCode.Empty: return Expression.Call(builder, Types.ObjectBuilder.PutNull, name);
                case TypeCode.Object:
                    if (valueType.IsValueType)
                    {
                        if (valueType.IsEnum)
                        {
                            // enum
                        }
                        else
                        {
                            // struct or nullable
                        }
                    }
                    else
                    {
                        // class
                    }
                    throw new NotImplementedException();
                case TypeCode.DBNull: return Expression.Call(builder, Types.ObjectBuilder.PutNull, name);
                case TypeCode.Boolean: return Expression.Call(builder, Types.ObjectBuilder.PutBoolean, name, value);
                case TypeCode.Char: return Expression.Call(builder, Types.ObjectBuilder.PutString, name, Expression.Call(value, Types.Object.ToString));
                case TypeCode.SByte: return Expression.Call(builder, Types.ObjectBuilder.PutInt32, name, Expression.Convert(value, typeof(Int32)));
                case TypeCode.Byte: return Expression.Call(builder, Types.ObjectBuilder.PutInt32, name, Expression.Convert(value, typeof(Int32)));
                case TypeCode.Int16: return Expression.Call(builder, Types.ObjectBuilder.PutInt32, name, Expression.Convert(value, typeof(Int32)));
                case TypeCode.UInt16: return Expression.Call(builder, Types.ObjectBuilder.PutInt32, name, Expression.Convert(value, typeof(Int32)));
                case TypeCode.Int32: return Expression.Call(builder, Types.ObjectBuilder.PutInt32, name, value);
                case TypeCode.UInt32: return Expression.Call(builder, Types.ObjectBuilder.PutInt64, name, Expression.Convert(value, typeof(Int64)));
                case TypeCode.Int64: return Expression.Call(builder, Types.ObjectBuilder.PutInt64, name, value);
                case TypeCode.UInt64: return Expression.Call(builder, Types.ObjectBuilder.PutDecimal, name, Expression.Convert(value, typeof(decimal)));
                case TypeCode.Single: return Expression.Call(builder, Types.ObjectBuilder.PutSingle, name, value);
                case TypeCode.Double: return Expression.Call(builder, Types.ObjectBuilder.PutDouble, name, value);
                case TypeCode.Decimal: return Expression.Call(builder, Types.ObjectBuilder.PutDecimal, name, value);
                case TypeCode.DateTime: return Expression.Call(builder, Types.ObjectBuilder.PutString, name, Expression.Call(value, Types.DateTime.ToString, dateTimeFormat));
                case TypeCode.String: return Expression.Call(builder, Types.ObjectBuilder.PutString, name, value);
                default: throw new InvalidOperationException();
            }
        }

        private Expression EmitPushStatement(Expression builder, Expression instance, JsonMember member)
            => EmitPushStatement(builder, member.EmitAccessExpression(instance), member.ValueType, member.ValueTypeCode);
        private Expression EmitPushStatement(Expression builder, Expression value, Type valueType, TypeCode valueTypeCode)
        {
            switch (valueTypeCode)
            {
                case TypeCode.Empty: return Expression.Call(builder, Types.ArrayBuilder.PushNull);
                case TypeCode.Object:
                    if (valueType.IsValueType)
                    {
                        if (valueType.IsEnum)
                        {
                            // enum
                        }
                        else
                        {
                            // struct or nullable
                        }
                    }
                    else
                    {
                        // class
                    }
                    throw new NotImplementedException();
                case TypeCode.DBNull: return Expression.Call(builder, Types.ArrayBuilder.PushNull);
                case TypeCode.Boolean: return Expression.Call(builder, Types.ArrayBuilder.PushBoolean, value);
                case TypeCode.Char: return Expression.Call(builder, Types.ArrayBuilder.PushString, Expression.Call(value, Types.Object.ToString));
                case TypeCode.SByte: return Expression.Call(builder, Types.ArrayBuilder.PushInt32, Expression.Convert(value, typeof(Int32)));
                case TypeCode.Byte: return Expression.Call(builder, Types.ArrayBuilder.PushInt32, Expression.Convert(value, typeof(Int32)));
                case TypeCode.Int16: return Expression.Call(builder, Types.ArrayBuilder.PushInt32, Expression.Convert(value, typeof(Int32)));
                case TypeCode.UInt16: return Expression.Call(builder, Types.ArrayBuilder.PushInt32, Expression.Convert(value, typeof(Int32)));
                case TypeCode.Int32: return Expression.Call(builder, Types.ArrayBuilder.PushInt32, value);
                case TypeCode.UInt32: return Expression.Call(builder, Types.ArrayBuilder.PushInt64, Expression.Convert(value, typeof(Int64)));
                case TypeCode.Int64: return Expression.Call(builder, Types.ArrayBuilder.PushInt64, value);
                case TypeCode.UInt64: return Expression.Call(builder, Types.ArrayBuilder.PushDecimal, Expression.Convert(value, typeof(decimal)));
                case TypeCode.Single: return Expression.Call(builder, Types.ArrayBuilder.PushSingle, value);
                case TypeCode.Double: return Expression.Call(builder, Types.ArrayBuilder.PushDouble, value);
                case TypeCode.Decimal: return Expression.Call(builder, Types.ArrayBuilder.PushDecimal, value);
                case TypeCode.DateTime: return Expression.Call(builder, Types.ArrayBuilder.PushString, Expression.Call(value, Types.DateTime.ToString, dateTimeFormat));
                case TypeCode.String: return Expression.Call(builder, Types.ArrayBuilder.PushString, value);
                default: throw new InvalidOperationException();
            }
        }

        private static bool IsNotNull<T>(T obj) => typeof(T).IsValueType ? true : !ReferenceEquals(obj, null);

        private delegate JValue BuildValue<T>(T obj);
        private delegate void BuildArray<T>(JValue.ArrayBuilder builder, T obj);
        private delegate void BuildObject<T>(JValue.ObjectBuilder builder, T obj);
        private abstract class TypeContract
        {
            public Type Type { get; }

            protected TypeContract(Type type)
            {
                Type = type;
            }

            public abstract JValue Build(StringBuilder stringBuilder, object obj);
        }

        private class TypeContract<T> : TypeContract
        {
            private readonly BuildValue<T> buildValue;
            private readonly BuildArray<T> buildArray;
            private readonly BuildObject<T> buildObject;

            public TypeContract(BuildValue<T> buildValue)
                : base(typeof(T))
            {
                this.buildValue = buildValue;
            }

            public TypeContract(BuildArray<T> buildArray)
                : base(typeof(T))
            {
                this.buildArray = buildArray;
            }

            public TypeContract(BuildObject<T> buildObject)
                : base(typeof(T))
            {
                this.buildObject = buildObject;
            }

            public sealed override JValue Build(StringBuilder stringBuilder, object obj) => Build(stringBuilder, (T)obj);
            public virtual JValue Build(StringBuilder stringBuilder, T obj)
            {
                if (buildObject != null)
                {
                    var builder = new JValue.ObjectBuilder(stringBuilder);
                    BuildObject(builder, obj);
                    return builder.Build();
                }

                if (buildArray != null)
                {
                    var builder = new JValue.ArrayBuilder(stringBuilder);
                    BuildArray(builder, obj);
                    return builder.Build();
                }

                if (buildValue != null)
                    return buildValue(obj);

                throw new InvalidOperationException();
            }

            private JValue BuildValue(T obj) => buildValue(obj);
            private void BuildArray(JValue.ArrayBuilder builder, T obj) => buildArray(builder, obj);
            private void BuildObject(JValue.ObjectBuilder builder, T obj) => buildObject(builder, obj);
        }

        private sealed class JsonMember
        {
            public readonly ConstantExpression NameExpression;
            public readonly MemberInfo Member;
            public readonly Type ValueType;
            public readonly TypeCode ValueTypeCode;
            public readonly Func<Expression, Expression> EmitAccessExpression;

            public string Name => (string)NameExpression.Value;

            public JsonMember(string name, FieldInfo field) : this(
                name,
                field,
                field.FieldType,
                expression => Expression.Field(expression, field))
            {
            }

            public JsonMember(string name, PropertyInfo property) : this(
                name,
                property,
                property.PropertyType,
                expression => Expression.Property(expression, property))
            {
            }

            private JsonMember(string name, MemberInfo member, Type valueType, Func<Expression, Expression> emitAccessExpression)
            {
                this.NameExpression = Expression.Constant(name);
                this.Member = member;
                this.ValueType = valueType;
                this.ValueTypeCode = Type.GetTypeCode(ValueType);
                this.EmitAccessExpression = emitAccessExpression;
            }
        }

        private sealed class TypeComparer : IEqualityComparer<Type>
        {
            public static readonly TypeComparer Singleton = new TypeComparer();

            public bool Equals(Type x, Type y) => ReferenceEquals(x, y);
            public int GetHashCode(Type obj) => obj.GetHashCode();
        }
    }
}
