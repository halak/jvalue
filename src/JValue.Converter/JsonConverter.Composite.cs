using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Halak
{
    partial class JsonConverter
    {
        private sealed class CompositeObjectContract : TypeContract<CompositeObject>
        {
            private readonly JsonConverter converter;
            private readonly ArrayPool<Type> typeArrayPool;
            private readonly ConcurrentDictionary<ArraySegment<Type>, TypeContract<CompositeObject>> compositeTypeTable;
            private readonly Func<ArraySegment<Type>, TypeContract<CompositeObject>> compositeTypeContractFactory;

            public CompositeObjectContract(JsonConverter converter)
                : base(null)
            {
                this.converter = converter;
                this.typeArrayPool = ArrayPool<Type>.Shared;
                this.compositeTypeTable = new ConcurrentDictionary<ArraySegment<Type>, TypeContract<CompositeObject>>(CompositeTypeComparer.Singleton);
                this.compositeTypeContractFactory = CreateComposite;
            }

            public override JValue Build(StringBuilder stringBuilder, CompositeObject obj)
                => obj.Elements != null && obj.Elements.Count > 0 ? GetContract(obj).Build(stringBuilder, obj) : JValue.Null;

            private TypeContract<CompositeObject> GetContract(CompositeObject obj)
            {
                var elements = obj.Elements;
                var typeArray = typeArrayPool.Rent(elements.Count);
                try
                {
                    for (var i = 0; i < elements.Count; i++)
                        typeArray[i] = elements[i].GetType();
                    var types = new ArraySegment<Type>(typeArray, 0, obj.Elements.Count);
                    if (compositeTypeTable.TryGetValue(types, out var contract))
                        return contract;
                    else
                        return compositeTypeTable.GetOrAdd(Clone(types), compositeTypeContractFactory);
                }
                finally
                {
                    typeArrayPool.Return(typeArray);
                }
            }

            private TypeContract<CompositeObject> CreateComposite(ArraySegment<Type> types)
            {
                var jsonMembers = new Dictionary<string, (int InstanceIndex, JsonMember Member, int Order)>(StringComparer.Ordinal);
                for (int i = 0, order = 0; i < types.Count; i++)
                {
                    foreach (var member in converter.GetJsonMembers(types.Array[i + types.Offset]))
                    {
                        var memberOrder = order++;
                        if ((true/* preserve confliced member order */) && jsonMembers.TryGetValue(member.Name, out var conflicted))
                            memberOrder = conflicted.Order;

                        jsonMembers[member.Name] = (i, member, memberOrder);
                    }
                }

                var parameters = converter.EmitBuildObjectParameters(typeof(CompositeObject));

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
                    statements.Add(converter.EmitPutStatement(parameters.Builder, instances[instanceIndex], member));

                var compiledMethod = Expression.Lambda<BuildObject<CompositeObject>>(
                    Expression.Block(instances, statements), parameters.Builder, parameters.Source).Compile();

                return new TypeContract<CompositeObject>(compiledMethod);
            }

            private static ArraySegment<T> Clone<T>(ArraySegment<T> segment)
            {
                var clone = new T[segment.Count];
                for (var i = 0; i < clone.Length; i++)
                    clone[i] = segment.Array[i + segment.Offset];
                return new ArraySegment<T>(clone);
            }

            private sealed class CompositeTypeComparer : IEqualityComparer<ArraySegment<Type>>
            {
                public static readonly CompositeTypeComparer Singleton = new CompositeTypeComparer();

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
}
