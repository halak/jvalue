using System;
using System.Buffers;
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
        private delegate JValue BuildValue<T>(T obj);
        private delegate void BuildArray<T>(JValue.ArrayBuilder builder, T obj);
        private delegate void BuildObject<T>(JValue.ObjectBuilder builder, T obj);

        private readonly ArrayPool<Type> typeArrayPool;
        private readonly ConcurrentBag<StringBuilder> stringBuilderPool;
        private readonly ConcurrentDictionary<ArraySegment<Type>, TypeContract> typeTable;
        private readonly Func<ArraySegment<Type>, TypeContract> contractFactory;
        private readonly ConcurrentDictionary<Type, JsonMember[]> jsonMembersCache;
        private readonly Func<Type, JsonMember[]> jsonMembersFactory;
        private readonly MemberTypes targetMembers;
        private readonly ConstantExpression dateTimeFormat;

        public string DateTimeFormat => (string)dateTimeFormat.Value;

        public JsonConverter(MemberTypes targetMembers = MemberTypes.Property, string dateTimeFormat = "O")
        {
            targetMembers &= (MemberTypes.Property | MemberTypes.Field);

            this.typeArrayPool = ArrayPool<Type>.Shared;
            this.stringBuilderPool = new ConcurrentBag<StringBuilder>();
            this.typeTable = new ConcurrentDictionary<ArraySegment<Type>, TypeContract>(GetBuiltinContracts(), TypesComparer.Singleton);
            this.jsonMembersCache = new ConcurrentDictionary<Type, JsonMember[]>();
            this.contractFactory = Create;
            this.jsonMembersFactory = GetJsonMembersInternal;
            this.targetMembers = targetMembers;
            this.dateTimeFormat = Expression.Constant(dateTimeFormat);
        }

        public JValue FromObject(object obj, Type type = null) => (obj != null) ? SerializeInternal(GetContract(type ?? obj.GetType()), obj) : JValue.Null;
        public JValue FromObject<T>(T obj) => IsNotNull(obj) ? SerializeInternal(GetContract<T>(), obj) : JValue.Null;
        public JValue FromObject<T>(T? obj) where T : struct => obj.HasValue ? SerializeInternal(GetContract<T>(), obj.Value) : JValue.Null;
        public JValue FromObject(CompositeObject obj)
        {
            if (obj.Elements == null || obj.Elements.Count == 0)
                return JValue.Null;
            var elements = obj.Elements;

            var typeArray = typeArrayPool.Rent(obj.Elements.Count);
            for (var i = 0; i < elements.Count; i++)
                typeArray[i] = elements[i].GetType();
            var contract = (TypeContract<CompositeObject>)GetContractAndReturnArray(new ArraySegment<Type>(typeArray, 0, obj.Elements.Count));
            return SerializeInternal(contract, obj);
        }

        private TypeContract GetContract(Type type)
        {
            var typeArray = typeArrayPool.Rent(1);
            typeArray[0] = type;
            return GetContractAndReturnArray(new ArraySegment<Type>(typeArray, 0, 1));
        }
        private TypeContract<T> GetContract<T>() => (TypeContract<T>)GetContract(typeof(T));

        private TypeContract GetContractAndReturnArray(ArraySegment<Type> types)
        {
            try
            {
                return typeTable.GetOrAdd(types, contractFactory);
            }
            finally
            {
                typeArrayPool.Return(types.Array);
            }
        }

        private JValue SerializeInternal(TypeContract contract, object obj)
        {
            var stringBuilder = RentStringBuilder();
            try
            {
                return contract.Build(RentStringBuilder(), obj);
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

        private TypeContract Create(ArraySegment<Type> types)
            => (types.Count == 1) ? Create(types.Array[types.Offset]) : CreateComposite(types);

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

        private TypeContract CreateComposite(ArraySegment<Type> types)
        {
            types = Clone(types);

            var jsonMembers = new Dictionary<string, (int InstanceIndex, JsonMember Member, int Order)>(StringComparer.Ordinal);
            for (int i = 0, order = 0; i < types.Count; i++)
            {
                foreach (var member in GetJsonMembers(types.Array[i + types.Offset]))
                {
                    var memberOrder = order++;
                    if ((true/* preserve confliced member order */) && jsonMembers.TryGetValue(member.Name, out var conflicted))
                        memberOrder = conflicted.Order;

                    jsonMembers[member.Name] = (i, member, memberOrder);
                }
            }

            var parameters = EmitBuildObjectParameters(typeof(CompositeObject));

            var elements = Expression.Property(parameters.Source, nameof(CompositeObject.Elements));
            var statements = new List<Expression>(jsonMembers.Count * 2);
            var instances = new ParameterExpression[types.Count];
            for (var i = 0; i < types.Count; i++)
            {
                var type = types.Array[i + types.Offset];
                var element = Expression.Property(elements, Types.IListObject.Indexer, Expression.Constant(i));
                instances[i] = Expression.Variable(type, $"instance{i}");
                statements.Add(Expression.Assign(instances[i], Expression.Convert(element, type)));
            }

            foreach (var (instanceIndex, member, _) in jsonMembers.Values.OrderBy(it => it.Order))
                statements.Add(EmitPutStatement(parameters.Builder, instances[instanceIndex], member));

            var compiledMethod = Expression.Lambda<BuildObject<CompositeObject>>(
                Expression.Block(instances, statements), parameters.Builder, parameters.Source).Compile();

            return new TypeContract<CompositeObject>(types, compiledMethod);
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

        private static ArraySegment<T> Clone<T>(ArraySegment<T> segment)
        {
            var clone = new T[segment.Count];
            for (var i = 0; i < clone.Length; i++)
                clone[i] = segment.Array[i + segment.Offset];
            return new ArraySegment<T>(clone);
        }

        private static bool IsNotNull<T>(T obj) => typeof(T).IsValueType ? true : !ReferenceEquals(obj, null);

        private abstract class TypeContract
        {
            private readonly ArraySegment<Type> types;

            public ArraySegment<Type> Types => types;

            public TypeContract(Type type)
                : this(new ArraySegment<Type>(new[] { type }))
            {
            }

            public TypeContract(ArraySegment<Type> types)
            {
                this.types = types;
            }

            public abstract JValue Build(StringBuilder stringBuilder, object obj);
        }

        private sealed class TypeContract<T> : TypeContract
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

            public TypeContract(ArraySegment<Type> types, BuildObject<T> buildObject)
                : base(types)
            {
                this.buildObject = buildObject;
            }

            public override JValue Build(StringBuilder stringBuilder, object obj) => Build(stringBuilder, (T)obj);
            public JValue Build(StringBuilder stringBuilder, T obj)
            {
                if (buildObject != null)
                {
                    var builder = new JValue.ObjectBuilder(stringBuilder);
                    BuildWith(builder, obj);
                    return builder.Build();
                }

                if (buildArray != null)
                {
                    var builder = new JValue.ArrayBuilder(stringBuilder);
                    BuildWith(builder, obj);
                    return builder.Build();
                }

                if (buildValue != null)
                    return buildValue(obj);

                throw new InvalidOperationException();
            }

            public JValue BuildValue(T obj) => buildValue(obj);
            public void BuildWith(JValue.ArrayBuilder builder, T obj) => buildArray(builder, obj);
            public void BuildWith(JValue.ObjectBuilder builder, T obj) => buildObject(builder, obj);
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

        private sealed class TypesComparer : IEqualityComparer<ArraySegment<Type>>
        {
            public static readonly TypesComparer Singleton = new TypesComparer();

            public bool Equals(ArraySegment<Type> x, ArraySegment<Type> y)
            {
                if (x.Count == y.Count)
                {
                    for (var i = 0; i < x.Count; i++)
                    {
                        if (x.Array[i + x.Offset] != y.Array[i + y.Offset])
                            return false;
                    }

                    return true;
                }
                else
                    return false;
            }

            public int GetHashCode(ArraySegment<Type> obj)
            {
                var hashCode = 0;
                for (var i = 0; i < obj.Count; i++)
                    hashCode ^= obj.Array[i + obj.Offset].GetHashCode();
                return hashCode;
            }
        }
    }
}
